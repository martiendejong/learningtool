using LearningTool.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;

    public CoursesController(
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService)
    {
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
    }

    [HttpGet("{courseId}")]
    public async Task<IActionResult> GetCourse(int courseId)
    {
        var course = await _knowledgeService.GetCourseByIdAsync(courseId);
        if (course == null) return NotFound();

        return Ok(course);
    }

    [HttpGet("topic/{topicId}")]
    public async Task<IActionResult> GetCoursesForTopic(int topicId)
    {
        var courses = await _knowledgeService.GetCoursesForTopicAsync(topicId);
        return Ok(courses);
    }

    [HttpGet("in-progress")]
    public async Task<IActionResult> GetInProgressCourses()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var courses = await _userLearningService.GetInProgressCoursesAsync(userId);
        return Ok(courses);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedCourses()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var courses = await _userLearningService.GetCompletedCoursesAsync(userId);
        return Ok(courses);
    }

    [HttpPost("{courseId}/start")]
    public async Task<IActionResult> StartCourse(int courseId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var userCourse = await _userLearningService.StartCourseAsync(userId, courseId);
        return Ok(userCourse);
    }

    [HttpPost("{courseId}/complete")]
    public async Task<IActionResult> CompleteCourse(int courseId, [FromBody] CompleteCourseRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var userCourse = await _userLearningService.CompleteCourseAsync(userId, courseId, request.AssessmentScore);
        return Ok(userCourse);
    }

    [HttpGet("user-course/{courseId}")]
    public async Task<IActionResult> GetUserCourse(int courseId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var userCourse = await _userLearningService.GetUserCourseAsync(userId, courseId);
        if (userCourse == null) return NotFound();

        return Ok(userCourse);
    }
}

public record CompleteCourseRequest(int AssessmentScore);
