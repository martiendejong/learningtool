using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hazina.API.Generic.Dynamic;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly DynamicEntityStore _store;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        DynamicEntityStore store,
        IConfiguration configuration,
        ILogger<ChatController> logger)
    {
        _store = store;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Send a message to the AI assistant
    /// </summary>
    [HttpPost("message")]
    public async Task<ActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Save user message using DynamicEntity
        var userMessage = new DynamicEntity();
        userMessage["userId"] = userId;
        userMessage["role"] = "user";
        userMessage["content"] = request.Message;
        if (request.CourseId.HasValue)
        {
            userMessage["courseId"] = request.CourseId.Value;
        }

        await _store.CreateAsync("ChatMessage", userMessage);

        // Get OpenAI configuration
        var apiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

        // Initialize OpenAI client
        var client = new OpenAIClient(new ApiKeyCredential(apiKey));
        var chatClient = client.GetChatClient(model);

        // Build conversation history
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateSystemMessage(@"You are an intelligent AI learning assistant.
Help users build learning paths and answer questions about courses.
When users want to learn something, guide them through skills, topics, and courses.")
        };

        // Add recent chat history (last 10 messages)
        // Note: DynamicEntityStore doesn't have QueryAsync, using GetAllAsync instead
        var allMessages = await _store.GetAllAsync("ChatMessage", 1, 100);

        // Filter by userId and courseId
        var filteredMessages = allMessages
            .Where(m => m["userId"]?.ToString() == userId)
            .Where(m => request.CourseId.HasValue
                ? m["courseId"]?.ToString() == request.CourseId.Value.ToString()
                : m["courseId"] == null)
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        foreach (var msg in filteredMessages)
        {
            var role = msg["role"]?.ToString() ?? "user";
            var content = msg["content"]?.ToString() ?? "";

            messages.Add(role == "user"
                ? ChatMessage.CreateUserMessage(content)
                : ChatMessage.CreateAssistantMessage(content));
        }

        // Add current message
        messages.Add(ChatMessage.CreateUserMessage(request.Message));

        // Call OpenAI with tools
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 800,
            Temperature = 0.7f
        };

        // Add tools to the options (Tools is a collection, not assignable)
        options.Tools.Add(ChatTool.CreateFunctionTool(
            functionName: "add_skill",
            functionDescription: "Add a skill to the user's learning path",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "skillId": {
                        "type": "string",
                        "description": "The ID of the skill to add"
                    },
                    "skillName": {
                        "type": "string",
                        "description": "The name of the skill (for context)"
                    }
                },
                "required": ["skillId"]
            }
            """)));

        options.Tools.Add(ChatTool.CreateFunctionTool(
            functionName: "search_skills",
            functionDescription: "Search for available skills by name or description",
            functionParameters: BinaryData.FromString("""
            {
                "type": "object",
                "properties": {
                    "query": {
                        "type": "string",
                        "description": "Search query to find skills"
                    }
                },
                "required": ["query"]
            }
            """)));

        var completion = await chatClient.CompleteChatAsync(messages, options);

        // Check if AI wants to call functions
        var toolCalls = completion.Value.ToolCalls;
        if (toolCalls != null && toolCalls.Count > 0)
        {
            // Process function calls
            var functionResults = new List<object>();

            foreach (var toolCall in toolCalls)
            {
                if (toolCall is ChatToolCall functionCall)
                {
                    var functionName = functionCall.FunctionName;
                    var functionArgs = functionCall.FunctionArguments;

                    string result;
                    try
                    {
                        result = await ExecuteFunctionAsync(userId, functionName, functionArgs);
                    }
                    catch (Exception ex)
                    {
                        result = $"Error: {ex.Message}";
                    }

                    functionResults.Add(new
                    {
                        toolCallId = functionCall.Id,
                        function = functionName,
                        result = result
                    });

                    // Add function call and result to conversation
                    messages.Add(ChatMessage.CreateAssistantMessage(toolCalls));
                    messages.Add(ChatMessage.CreateToolMessage(functionCall.Id, result));
                }
            }

            // Call OpenAI again with function results
            var secondCompletion = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
            {
                MaxOutputTokenCount = 800,
                Temperature = 0.7f
            });

            var finalMessage = secondCompletion.Value.Content[0].Text;

            // Save assistant response
            var assistantMsg = new DynamicEntity();
            assistantMsg["userId"] = userId;
            assistantMsg["role"] = "assistant";
            assistantMsg["content"] = finalMessage;
            if (request.CourseId.HasValue)
            {
                assistantMsg["courseId"] = request.CourseId.Value;
            }

            await _store.CreateAsync("ChatMessage", assistantMsg);

            return Ok(new
            {
                Message = finalMessage,
                RequiresAction = false,
                ToolCalls = functionResults
            });
        }

        // No function calls - return normal message
        var assistantMessage = completion.Value.Content[0].Text;

        // Save assistant response
        var assistantMsgNormal = new DynamicEntity();
        assistantMsgNormal["userId"] = userId;
        assistantMsgNormal["role"] = "assistant";
        assistantMsgNormal["content"] = assistantMessage;
        if (request.CourseId.HasValue)
        {
            assistantMsgNormal["courseId"] = request.CourseId.Value;
        }

        await _store.CreateAsync("ChatMessage", assistantMsgNormal);

        return Ok(new
        {
            Message = assistantMessage,
            RequiresAction = false
        });
    }

    /// <summary>
    /// Get chat history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 50, [FromQuery] int? courseId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var allMessages = await _store.GetAllAsync("ChatMessage", 1, 200);

        var filteredMessages = allMessages
            .Where(m => m["userId"]?.ToString() == userId)
            .Where(m => courseId.HasValue
                ? m["courseId"]?.ToString() == courseId.Value.ToString()
                : m["courseId"] == null)
            .OrderBy(m => m.CreatedAt)
            .Take(limit)
            .Select(m => new
            {
                Role = m["role"]?.ToString() ?? "user",
                Content = m["content"]?.ToString() ?? "",
                Timestamp = m.CreatedAt
            })
            .ToList();

        return Ok(filteredMessages);
    }

    /// <summary>
    /// Execute AI function calls
    /// </summary>
    private async Task<string> ExecuteFunctionAsync(string userId, string functionName, BinaryData functionArgs)
    {
        var argsJson = functionArgs.ToString();

        switch (functionName)
        {
            case "add_skill":
                var addSkillArgs = JsonSerializer.Deserialize<JsonElement>(argsJson);
                var skillId = addSkillArgs.GetProperty("skillId").GetString();

                if (string.IsNullOrEmpty(skillId))
                {
                    return "Error: skillId is required";
                }

                if (!Guid.TryParse(skillId, out var skillGuid))
                {
                    return "Error: Invalid skill ID format";
                }

                // Check if user already has this skill
                var allUserSkills = await _store.GetAllAsync("UserSkill", 1, 1000);
                var existing = allUserSkills.FirstOrDefault(us =>
                    us["userId"]?.ToString() == userId &&
                    us["skillId"]?.ToString() == skillId);

                if (existing != null)
                {
                    return "User already has this skill";
                }

                // Get skill details
                var skill = await _store.GetByIdAsync("Skill", skillGuid);
                if (skill == null)
                {
                    return "Skill not found";
                }

                // Create UserSkill
                var userSkill = new DynamicEntity();
                userSkill["userId"] = userId;
                userSkill["skillId"] = skillId;
                userSkill["status"] = "Learning";
                userSkill["startedAt"] = DateTime.UtcNow.ToString("O");

                await _store.CreateAsync("UserSkill", userSkill);

                return $"Successfully added skill: {skill["name"]}";

            case "search_skills":
                var searchArgs = JsonSerializer.Deserialize<JsonElement>(argsJson);
                var query = searchArgs.GetProperty("query").GetString()?.ToLower() ?? "";

                var allSkills = await _store.GetAllAsync("Skill", 1, 100);
                var matchingSkills = allSkills
                    .Where(s =>
                        s["name"]?.ToString()?.ToLower().Contains(query) == true ||
                        s["description"]?.ToString()?.ToLower().Contains(query) == true)
                    .Take(5)
                    .Select(s => new
                    {
                        id = s.Id,
                        name = s["name"]?.ToString(),
                        description = s["description"]?.ToString(),
                        difficulty = s["difficulty"]?.ToString()
                    })
                    .ToList();

                return JsonSerializer.Serialize(matchingSkills);

            default:
                return $"Unknown function: {functionName}";
        }
    }

    /// <summary>
    /// Clear chat history
    /// </summary>
    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory([FromQuery] int? courseId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var allMessages = await _store.GetAllAsync("ChatMessage", 1, 1000);

        var messagesToDelete = allMessages
            .Where(m => m["userId"]?.ToString() == userId)
            .Where(m => courseId.HasValue
                ? m["courseId"]?.ToString() == courseId.Value.ToString()
                : m["courseId"] == null)
            .ToList();

        foreach (var msg in messagesToDelete)
        {
            await _store.DeleteAsync("ChatMessage", msg.Id);
        }

        return Ok(new { message = $"Cleared {messagesToDelete.Count} messages" });
    }
}

public class ChatRequest
{
    public string Message { get; set; } = "";
    public int? CourseId { get; set; }
}
