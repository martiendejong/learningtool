using LearningTool.Application.DTOs;
using LearningTool.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;

    public ChatController(
        IChatService chatService,
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService)
    {
        _chatService = chatService;
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
    }

    /// <summary>
    /// Send a message to the AI assistant
    /// </summary>
    [HttpPost("message")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var response = await _chatService.ProcessMessageAsync(userId, request.Message);

        // If tools were called, execute them
        if (response.ToolCalls != null && response.ToolCalls.Any())
        {
            var results = await ExecuteToolCallsAsync(userId, response.ToolCalls);

            // Add tool results to response
            return Ok(new
            {
                response.Message,
                response.ToolCalls,
                response.RequiresAction,
                ToolResults = results
            });
        }

        return Ok(response);
    }

    /// <summary>
    /// Get chat history
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] int limit = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var history = await _chatService.GetChatHistoryAsync(userId, limit);
        return Ok(history);
    }

    private async Task<List<ToolResult>> ExecuteToolCallsAsync(string userId, List<ToolCall> toolCalls)
    {
        var results = new List<ToolResult>();

        foreach (var toolCall in toolCalls)
        {
            var result = await ExecuteToolAsync(userId, toolCall);
            results.Add(result);
        }

        return results;
    }

    private async Task<ToolResult> ExecuteToolAsync(string userId, ToolCall toolCall)
    {
        try
        {
            switch (toolCall.ToolName)
            {
                case "add_skill":
                    {
                        var name = toolCall.Arguments["name"].ToString()!;

                        var skill = await _knowledgeService.FindOrCreateSkillAsync(name);
                        var userSkill = await _userLearningService.AddSkillToUserAsync(userId, skill!.Id);

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Added skill: {name}",
                            Data = userSkill
                        };
                    }

                case "remove_skill":
                    {
                        var skillId = Convert.ToInt32(toolCall.Arguments["skillId"]);
                        await _userLearningService.RemoveSkillFromUserAsync(userId, skillId);

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Removed skill"
                        };
                    }

                case "add_topic":
                    {
                        var skillId = Convert.ToInt32(toolCall.Arguments["skillId"]);
                        var name = toolCall.Arguments["name"].ToString()!;
                        var description = toolCall.Arguments.ContainsKey("description")
                            ? toolCall.Arguments["description"].ToString() ?? ""
                            : "";

                        var topic = await _knowledgeService.AddTopicAsync(skillId, name, description);

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Added topic: {name}",
                            Data = topic
                        };
                    }

                case "get_user_skills":
                    {
                        var skills = await _userLearningService.GetUserSkillsAsync(userId);

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Retrieved {skills.Count} skills",
                            Data = skills
                        };
                    }

                default:
                    return new ToolResult
                    {
                        ToolCallId = toolCall.Id,
                        Success = false,
                        Result = $"Unknown tool: {toolCall.ToolName}"
                    };
            }
        }
        catch (Exception ex)
        {
            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                Success = false,
                Result = $"Error executing tool: {ex.Message}"
            };
        }
    }
}
