using LearningTool.Application.DTOs;
using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using System.Text.Json;
using System.ClientModel;
using OpenAI;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using DTOToolCall = LearningTool.Application.DTOs.ToolCall;
using OpenAIChatMessage = OpenAI.Chat.ChatMessage;
using DomainChatMessage = LearningTool.Domain.Entities.ChatMessage;

namespace LearningTool.Application.Services;

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message);
    Task<List<DomainChatMessage>> GetChatHistoryAsync(string userId, int limit = 50);
    Task ClearChatHistoryAsync(string userId);
}

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _systemPrompt;
    private readonly int _maxTokens;
    private readonly float _temperature;

    public ChatService(
        IChatMessageRepository chatMessageRepository,
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _chatMessageRepository = chatMessageRepository;
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;

        // Read OpenAI configuration
        _apiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key not configured");
        _model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        _systemPrompt = configuration["OpenAI:SystemPrompt"] ?? "You are a helpful AI learning assistant.";
        _maxTokens = int.Parse(configuration["OpenAI:MaxTokens"] ?? "500");
        _temperature = float.Parse(configuration["OpenAI:Temperature"] ?? "0.7", System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
    {
        // Save user message
        var userMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(userMessage);

        // Process message with OpenAI
        var response = await GenerateResponseAsync(userId, message);

        // Save assistant message
        var assistantMessage = new DomainChatMessage
        {
            UserId = userId,
            Role = "assistant",
            Content = response.Message,
            ToolCalls = response.ToolCalls != null ? JsonSerializer.Serialize(response.ToolCalls) : null,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(assistantMessage);

        return response;
    }

    public async Task<List<DomainChatMessage>> GetChatHistoryAsync(string userId, int limit = 50)
    {
        return await _chatMessageRepository.GetByUserIdAsync(userId, limit);
    }

    public async Task ClearChatHistoryAsync(string userId)
    {
        await _chatMessageRepository.DeleteByUserIdAsync(userId);
    }

    private async Task<ChatResponse> GenerateResponseAsync(string userId, string message)
    {
        // Initialize OpenAI client
        var client = new OpenAIClient(new ApiKeyCredential(_apiKey));
        var chatClient = client.GetChatClient(_model);

        // Get conversation history
        var history = await GetChatHistoryAsync(userId, 20);

        // Get user's current skills for context
        var userSkills = await _userLearningService.GetUserSkillsAsync(userId);
        var skillsContext = userSkills.Any()
            ? $"\n\nUser is currently learning: {string.Join(", ", userSkills.Select(s => s.Skill.Name))}"
            : "\n\nUser hasn't started learning any skills yet.";

        // Build messages array
        var messages = new List<OpenAIChatMessage>();

        // System message with context
        messages.Add(OpenAIChatMessage.CreateSystemMessage(_systemPrompt + skillsContext));

        // Add conversation history
        foreach (var historyMsg in history.OrderBy(m => m.Timestamp))
        {
            if (historyMsg.Role == "user")
            {
                messages.Add(OpenAIChatMessage.CreateUserMessage(historyMsg.Content));
            }
            else if (historyMsg.Role == "assistant")
            {
                messages.Add(OpenAIChatMessage.CreateAssistantMessage(historyMsg.Content));
            }
        }

        // Add current user message
        messages.Add(OpenAIChatMessage.CreateUserMessage(message));

        // Define available functions
        var tools = new List<ChatTool>
        {
            ChatTool.CreateFunctionTool(
                functionName: "add_skill",
                functionDescription: "Add a new skill to the user's learning path",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "The name of the skill to add"
                        },
                        "description": {
                            "type": "string",
                            "description": "A brief description of what this skill involves"
                        },
                        "difficulty": {
                            "type": "string",
                            "enum": ["Beginner", "Intermediate", "Advanced"],
                            "description": "The difficulty level of the skill"
                        }
                    },
                    "required": ["name", "description"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "remove_skill",
                functionDescription: "Remove a skill from the user's learning path",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "The name of the skill to remove"
                        }
                    },
                    "required": ["name"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "get_user_progress",
                functionDescription: "Get the user's learning progress and statistics",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {},
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "list_available_topics",
                functionDescription: "List topics available for a specific skill",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "skillName": {
                            "type": "string",
                            "description": "The name of the skill to get topics for"
                        }
                    },
                    "required": ["skillName"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "add_topic",
                functionDescription: "Add a new topic to an existing skill",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "skillName": {
                            "type": "string",
                            "description": "The name of the skill to add the topic to"
                        },
                        "topicName": {
                            "type": "string",
                            "description": "The name of the topic to add"
                        },
                        "description": {
                            "type": "string",
                            "description": "A brief description of what this topic covers"
                        }
                    },
                    "required": ["skillName", "topicName", "description"],
                    "additionalProperties": false
                }
                """)
            ),
            ChatTool.CreateFunctionTool(
                functionName: "add_course",
                functionDescription: "Add a course recommendation to a specific topic",
                functionParameters: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "topicName": {
                            "type": "string",
                            "description": "The name of the topic to add the course to"
                        },
                        "courseName": {
                            "type": "string",
                            "description": "The name of the course"
                        },
                        "url": {
                            "type": "string",
                            "description": "The URL to the course"
                        },
                        "description": {
                            "type": "string",
                            "description": "A brief description of the course content"
                        }
                    },
                    "required": ["topicName", "courseName", "description"],
                    "additionalProperties": false
                }
                """)
            )
        };

        // Create chat completion options
        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _maxTokens,
            Temperature = _temperature
        };

        foreach (var tool in tools)
        {
            options.Tools.Add(tool);
        }

        // Call OpenAI API
        var completion = await chatClient.CompleteChatAsync(messages, options);

        var responseMessage = string.Empty;
        var toolCalls = new List<DTOToolCall>();

        // Check if the model wants to call functions
        if (completion.Value.FinishReason == ChatFinishReason.ToolCalls)
        {
            foreach (var toolCall in completion.Value.ToolCalls)
            {
                var functionToolCall = toolCall as ChatToolCall;
                if (functionToolCall != null)
                {
                    // Parse function arguments
                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(
                        functionToolCall.FunctionArguments.ToString());

                    toolCalls.Add(new DTOToolCall
                    {
                        Id = functionToolCall.Id,
                        ToolName = functionToolCall.FunctionName,
                        Arguments = args ?? new Dictionary<string, object>()
                    });
                }
            }

            // Execute tool calls and get results
            var toolResults = new List<string>();
            foreach (var toolCall in toolCalls)
            {
                var result = await ExecuteToolCallAsync(userId, toolCall);
                toolResults.Add(result);
            }

            // Generate final response with tool results
            var finalMessages = new List<OpenAIChatMessage>(messages);

            // Add assistant message with tool calls
            var assistantMessage = OpenAIChatMessage.CreateAssistantMessage(completion.Value.ToolCalls);
            finalMessages.Add(assistantMessage);

            // Add tool results
            foreach (var (toolCall, result) in toolCalls.Zip(toolResults))
            {
                finalMessages.Add(OpenAIChatMessage.CreateToolMessage(toolCall.Id, result));
            }

            // Get final response from OpenAI
            var finalCompletion = await chatClient.CompleteChatAsync(finalMessages);
            responseMessage = finalCompletion.Value.Content.Count > 0 ? finalCompletion.Value.Content[0].Text : string.Empty;
        }
        else
        {
            // No tool calls, get text response directly
            responseMessage = completion.Value.Content.Count > 0 ? completion.Value.Content[0].Text : string.Empty;
        }

        return new ChatResponse
        {
            Message = responseMessage,
            RequiresAction = toolCalls.Any(),
            ToolCalls = toolCalls.Any() ? toolCalls : null
        };
    }

    private async Task<string> ExecuteToolCallAsync(string userId, DTOToolCall toolCall)
    {
        try
        {
            switch (toolCall.ToolName)
            {
                case "add_skill":
                    var name = toolCall.Arguments["name"].ToString()!;
                    var description = toolCall.Arguments["description"].ToString()!;
                    var difficulty = toolCall.Arguments.ContainsKey("difficulty")
                        ? toolCall.Arguments["difficulty"].ToString()!
                        : "Beginner";

                    // Check if skill already exists in the system
                    var existingSkills = await _knowledgeService.GetAllSkillsAsync();
                    var skill = existingSkills.FirstOrDefault(s =>
                        s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                    if (skill == null)
                    {
                        // Create new skill in the knowledge base
                        var difficultyEnum = difficulty.ToLower() switch
                        {
                            "beginner" => DifficultyLevel.Beginner,
                            "intermediate" => DifficultyLevel.Intermediate,
                            "advanced" => DifficultyLevel.Advanced,
                            _ => DifficultyLevel.Beginner
                        };
                        skill = await _knowledgeService.AddSkillToCatalogAsync(name, description, difficultyEnum);
                    }

                    // Add to user's learning path
                    await _userLearningService.AddSkillToUserAsync(userId, skill.Id);

                    return $"Successfully added '{name}' to your learning path.";

                case "remove_skill":
                    var skillName = toolCall.Arguments["name"].ToString()!;
                    var userSkills = await _userLearningService.GetUserSkillsAsync(userId);
                    var userSkill = userSkills.FirstOrDefault(us =>
                        us.Skill.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase));

                    if (userSkill != null)
                    {
                        await _userLearningService.RemoveSkillFromUserAsync(userId, userSkill.SkillId);
                        return $"Successfully removed '{skillName}' from your learning path.";
                    }
                    return $"Skill '{skillName}' not found in your learning path.";

                case "get_user_progress":
                    var skills = await _userLearningService.GetUserSkillsAsync(userId);
                    var completedCourses = await _userLearningService.GetCompletedCoursesAsync(userId);

                    return $"You are currently learning {skills.Count} skill(s) and have completed {completedCourses.Count} course(s).";

                case "list_available_topics":
                    var targetSkill = toolCall.Arguments["skillName"].ToString()!;
                    var allSkills = await _knowledgeService.GetAllSkillsAsync();
                    var foundSkill = allSkills.FirstOrDefault(s =>
                        s.Name.Equals(targetSkill, StringComparison.OrdinalIgnoreCase));

                    if (foundSkill != null)
                    {
                        var topics = await _knowledgeService.GetTopicsForSkillAsync(foundSkill.Id);
                        if (topics.Any())
                        {
                            var topicList = string.Join(", ", topics.Select(t => t.Name));
                            return $"Topics for {targetSkill}: {topicList}";
                        }
                        return $"No topics found for {targetSkill} yet.";
                    }
                    return $"Skill '{targetSkill}' not found.";

                case "add_topic":
                    var skillNameForTopic = toolCall.Arguments["skillName"].ToString()!;
                    var topicName = toolCall.Arguments["topicName"].ToString()!;
                    var topicDescription = toolCall.Arguments["description"].ToString()!;

                    var skillsForTopic = await _knowledgeService.GetAllSkillsAsync();
                    var skillForTopic = skillsForTopic.FirstOrDefault(s =>
                        s.Name.Equals(skillNameForTopic, StringComparison.OrdinalIgnoreCase));

                    if (skillForTopic != null)
                    {
                        var topic = await _knowledgeService.AddTopicAsync(skillForTopic.Id, topicName, topicDescription);
                        return $"Successfully added topic '{topicName}' to skill '{skillNameForTopic}'.";
                    }
                    return $"Skill '{skillNameForTopic}' not found. Please add the skill first.";

                case "add_course":
                    var topicNameForCourse = toolCall.Arguments["topicName"].ToString()!;
                    var courseName = toolCall.Arguments["courseName"].ToString()!;
                    var courseUrl = toolCall.Arguments.ContainsKey("url")
                        ? toolCall.Arguments["url"].ToString()!
                        : "";
                    var courseDescription = toolCall.Arguments["description"].ToString()!;

                    // Find the topic across all skills
                    var allSkillsForCourse = await _knowledgeService.GetAllSkillsAsync();
                    Domain.Entities.Topic? foundTopic = null;
                    foreach (var skillItem in allSkillsForCourse)
                    {
                        var topicsForSkill = await _knowledgeService.GetTopicsForSkillAsync(skillItem.Id);
                        foundTopic = topicsForSkill.FirstOrDefault(t =>
                            t.Name.Equals(topicNameForCourse, StringComparison.OrdinalIgnoreCase));
                        if (foundTopic != null) break;
                    }

                    if (foundTopic != null)
                    {
                        var resourceLinks = new List<ResourceLink>();
                        if (!string.IsNullOrEmpty(courseUrl))
                        {
                            resourceLinks.Add(new ResourceLink
                            {
                                Title = courseName,
                                Url = courseUrl,
                                Type = ResourceType.Tutorial
                            });
                        }

                        var course = await _knowledgeService.AddCourseAsync(
                            foundTopic.Id,
                            courseName,
                            courseDescription,
                            courseDescription, // Use description as content
                            60, // default 60 minutes
                            new List<string>(),
                            resourceLinks);
                        return $"Successfully added course '{courseName}' to topic '{topicNameForCourse}'.";
                    }
                    return $"Topic '{topicNameForCourse}' not found. Please add the topic first.";

                default:
                    return "Unknown tool call.";
            }
        }
        catch (Exception ex)
        {
            return $"Error executing {toolCall.ToolName}: {ex.Message}";
        }
    }
}
