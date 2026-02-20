using LearningTool.Application.DTOs;
using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using System.Text.Json;
using DTOToolCall = LearningTool.Application.DTOs.ToolCall;

namespace LearningTool.Application.Services;

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(string userId, string message);
    Task<List<ChatMessage>> GetChatHistoryAsync(string userId, int limit = 50);
}

public class ChatService : IChatService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly KnowledgeService _knowledgeService;
    private readonly UserLearningService _userLearningService;

    public ChatService(
        IChatMessageRepository chatMessageRepository,
        KnowledgeService knowledgeService,
        UserLearningService userLearningService)
    {
        _chatMessageRepository = chatMessageRepository;
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
    }

    public async Task<ChatResponse> ProcessMessageAsync(string userId, string message)
    {
        // Save user message
        var userMessage = new ChatMessage
        {
            UserId = userId,
            Role = "user",
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        await _chatMessageRepository.CreateAsync(userMessage);

        // Process message and determine tools to call
        var response = await GenerateResponseAsync(userId, message);

        // Save assistant message
        var assistantMessage = new ChatMessage
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

    public async Task<List<ChatMessage>> GetChatHistoryAsync(string userId, int limit = 50)
    {
        return await _chatMessageRepository.GetByUserIdAsync(userId, limit);
    }

    private async Task<ChatResponse> GenerateResponseAsync(string userId, string message)
    {
        // Simple rule-based chat for now (can be replaced with OpenAI API)
        message = message.ToLower();

        // Check for skill-related intents
        if (message.Contains("learn") || message.Contains("skill") || message.Contains("want to"))
        {
            var skills = await _userLearningService.GetUserSkillsAsync(userId);
            if (skills.Count == 0)
            {
                return new ChatResponse
                {
                    Message = "Great! I'd love to help you start your learning journey. What skill would you like to learn? For example, you could say 'I want to learn Machine Learning' or 'Help me learn Web Development'.",
                    RequiresAction = false
                };
            }
            else
            {
                var skillNames = string.Join(", ", skills.Select(s => s.Skill.Name));
                return new ChatResponse
                {
                    Message = $"You're currently learning: {skillNames}. Would you like to add another skill, or should we focus on these?",
                    RequiresAction = false
                };
            }
        }

        // Check for adding skill intent
        if (message.Contains("add skill") || message.Contains("learn "))
        {
            // Extract skill name (simplified)
            var skillName = ExtractSkillName(message);
            if (!string.IsNullOrEmpty(skillName))
            {
                return new ChatResponse
                {
                    Message = $"Great choice! Let me add '{skillName}' to your learning path.",
                    RequiresAction = true,
                    ToolCalls = new List<DTOToolCall>
                    {
                        new DTOToolCall
                        {
                            Id = Guid.NewGuid().ToString(),
                            ToolName = "add_skill",
                            Arguments = new Dictionary<string, object>
                            {
                                { "name", skillName },
                                { "description", $"Learn {skillName}" }
                            }
                        }
                    }
                };
            }
        }

        // Check for status/progress
        if (message.Contains("progress") || message.Contains("status") || message.Contains("how am i"))
        {
            var skills = await _userLearningService.GetUserSkillsAsync(userId);
            var completed = await _userLearningService.GetCompletedCoursesAsync(userId);

            return new ChatResponse
            {
                Message = $"Here's your progress: You're learning {skills.Count} skill(s) and have completed {completed.Count} course(s). Keep up the great work!",
                RequiresAction = false
            };
        }

        // Default greeting
        if (message.Contains("hello") || message.Contains("hi ") || message.Length < 10)
        {
            return new ChatResponse
            {
                Message = "Hello! I'm your AI learning assistant. I'm here to help you learn new skills. What would you like to learn today?",
                RequiresAction = false
            };
        }

        // Default response
        return new ChatResponse
        {
            Message = "I can help you learn new skills! Just tell me what you'd like to learn, and I'll guide you through the process. You can also ask about your progress or what courses are available.",
            RequiresAction = false
        };
    }

    private string ExtractSkillName(string message)
    {
        // Simple extraction - find text after "learn"
        var patterns = new[] { "learn ", "add skill ", "study " };

        foreach (var pattern in patterns)
        {
            var index = message.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var afterPattern = message.Substring(index + pattern.Length).Trim();
                // Take first few words or until punctuation
                var words = afterPattern.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    // Capitalize properly
                    return string.Join(" ", words.Take(Math.Min(3, words.Length)))
                        .Split(' ')
                        .Select(w => char.ToUpper(w[0]) + w.Substring(1))
                        .Aggregate((a, b) => a + " " + b);
                }
            }
        }

        return string.Empty;
    }
}
