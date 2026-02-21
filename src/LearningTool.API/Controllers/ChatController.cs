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

    /// <summary>
    /// Start a course and add initial message to chat
    /// </summary>
    [HttpPost("start-course")]
    public async Task<ActionResult<ChatResponse>> StartCourse([FromBody] StartCourseRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Get course details
        var course = await _knowledgeService.GetCourseByIdAsync(request.CourseId);
        if (course == null)
        {
            return NotFound(new { message = "Course not found" });
        }

        // Start course for user
        await _userLearningService.StartCourseAsync(userId, request.CourseId);

        // Clear chat history for clean start
        await _chatService.ClearChatHistoryAsync(userId);

        // Create initial chat message
        var message = $"I want to start the course '{course.Name}'. Please teach me step by step.";
        var response = await _chatService.ProcessMessageAsync(userId, message);

        return Ok(response);
    }

    /// <summary>
    /// Clear chat history
    /// </summary>
    [HttpDelete("history")]
    public async Task<IActionResult> ClearHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _chatService.ClearChatHistoryAsync(userId);
        return Ok(new { message = "Chat history cleared" });
    }

    /// <summary>
    /// Send a message in course-specific teaching mode
    /// </summary>
    [HttpPost("course/{courseId}/message")]
    public async Task<ActionResult<ChatResponse>> SendCourseMessage(int courseId, [FromBody] ChatRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var response = await _chatService.ProcessCourseMessageAsync(userId, courseId, request.Message);

        // If tools were called, execute them
        if (response.ToolCalls != null && response.ToolCalls.Any())
        {
            var results = await ExecuteCourseToolCallsAsync(userId, courseId, response.ToolCalls);

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
    /// Get course-specific chat history
    /// </summary>
    [HttpGet("course/{courseId}/history")]
    public async Task<IActionResult> GetCourseHistory(int courseId, [FromQuery] int limit = 50)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var history = await _chatService.GetCourseChatHistoryAsync(userId, courseId, limit);
        return Ok(history);
    }

    /// <summary>
    /// Clear course-specific chat history
    /// </summary>
    [HttpDelete("course/{courseId}/history")]
    public async Task<IActionResult> ClearCourseHistory(int courseId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        await _chatService.ClearCourseChatHistoryAsync(userId, courseId);
        return Ok(new { message = "Course chat history cleared" });
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

    private async Task<List<ToolResult>> ExecuteCourseToolCallsAsync(string userId, int courseId, List<ToolCall> toolCalls)
    {
        var results = new List<ToolResult>();

        foreach (var toolCall in toolCalls)
        {
            var result = await ExecuteCourseToolAsync(userId, courseId, toolCall);
            results.Add(result);
        }

        return results;
    }

    private async Task<ToolResult> ExecuteCourseToolAsync(string userId, int courseId, ToolCall toolCall)
    {
        try
        {
            switch (toolCall.ToolName)
            {
                case "get_course_content":
                    {
                        var course = await _knowledgeService.GetCourseByIdAsync(courseId);
                        if (course == null)
                        {
                            return new ToolResult
                            {
                                ToolCallId = toolCall.Id,
                                Success = false,
                                Result = "Course not found"
                            };
                        }

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Course: {course.Name}\nContent: {course.Content}",
                            Data = course
                        };
                    }

                case "mark_section_complete":
                    {
                        var sectionName = toolCall.Arguments["sectionName"].ToString();
                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Section '{sectionName}' marked as complete"
                        };
                    }

                case "get_student_progress":
                    {
                        var userCourse = await _userLearningService.GetUserCourseAsync(userId, courseId);
                        if (userCourse == null)
                        {
                            return new ToolResult
                            {
                                ToolCallId = toolCall.Id,
                                Success = true,
                                Result = "Not started yet",
                                Data = new { progressPercentage = 0, startedAt = (DateTime?)null }
                            };
                        }

                        var course = await _knowledgeService.GetCourseByIdAsync(courseId);
                        var progressPercentage = course != null && course.EstimatedMinutes > 0
                            ? Math.Min(100, (int)((userCourse.MinutesSpent / (float)course.EstimatedMinutes) * 100))
                            : 0;

                        return new ToolResult
                        {
                            ToolCallId = toolCall.Id,
                            Success = true,
                            Result = $"Progress: {progressPercentage}%",
                            Data = new { progressPercentage, minutesSpent = userCourse.MinutesSpent, status = userCourse.Status }
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

public record StartCourseRequest(int CourseId);
