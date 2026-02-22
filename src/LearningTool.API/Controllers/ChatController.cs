using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hazina.API.Generic.Dynamic;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

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

        // Call OpenAI
        var completion = await chatClient.CompleteChatAsync(messages, new ChatCompletionOptions
        {
            MaxOutputTokenCount = 800,
            Temperature = 0.7f
        });

        var assistantMessage = completion.Value.Content[0].Text;

        // Save assistant response
        var assistantMsg = new DynamicEntity();
        assistantMsg["userId"] = userId;
        assistantMsg["role"] = "assistant";
        assistantMsg["content"] = assistantMessage;
        if (request.CourseId.HasValue)
        {
            assistantMsg["courseId"] = request.CourseId.Value;
        }

        await _store.CreateAsync("ChatMessage", assistantMsg);

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
