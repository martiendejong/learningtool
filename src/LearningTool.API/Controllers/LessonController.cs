using Microsoft.AspNetCore.Mvc;
using Hazina.API.Generic.Dynamic;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{
    private readonly DynamicEntityStore _store;
    private readonly ILogger<LessonController> _logger;

    public LessonController(
        DynamicEntityStore store,
        ILogger<LessonController> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Get all lessons with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLessons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? topicId = null)
    {
        try
        {
            // Get all lessons from Hazina store
            var allLessons = await _store.GetAllAsync("Lesson", page, pageSize);

            // Filter by topicId if provided
            var lessons = topicId != null
                ? allLessons.Where(l => l["topicId"]?.ToString() == topicId).ToList()
                : allLessons;

            // Map to proper response format
            var result = lessons.Select(lesson => new
            {
                id = lesson.Id.ToString(),
                topicId = lesson["topicId"]?.ToString(),
                lessonNumber = lesson["lessonNumber"] != null ? Convert.ToInt32(lesson["lessonNumber"]) : (int?)null,
                title = lesson["title"]?.ToString(),
                module = lesson["module"]?.ToString(),
                htmlContent = lesson["htmlContent"]?.ToString(),
                slug = lesson["slug"]?.ToString(),
                estimatedMinutes = lesson["estimatedMinutes"] != null ? Convert.ToInt32(lesson["estimatedMinutes"]) : (int?)null,
                previousLessonId = lesson["previousLessonId"]?.ToString(),
                nextLessonId = lesson["nextLessonId"]?.ToString(),
                createdAt = lesson.CreatedAt,
                updatedAt = lesson.UpdatedAt
            })
            .OrderBy(l => l.lessonNumber)
            .ToList();

            return Ok(new
            {
                items = result,
                page,
                pageSize,
                totalCount = result.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lessons");
            return StatusCode(500, new { message = "Error fetching lessons", error = ex.Message });
        }
    }

    /// <summary>
    /// Get a single lesson by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetLesson(string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var lessonGuid))
            {
                return BadRequest(new { message = "Invalid lesson ID format" });
            }

            var lesson = await _store.GetByIdAsync("Lesson", lessonGuid);
            if (lesson == null)
            {
                return NotFound(new { message = "Lesson not found" });
            }

            var result = new
            {
                id = lesson.Id.ToString(),
                topicId = lesson["topicId"]?.ToString(),
                lessonNumber = lesson["lessonNumber"] != null ? Convert.ToInt32(lesson["lessonNumber"]) : (int?)null,
                title = lesson["title"]?.ToString(),
                module = lesson["module"]?.ToString(),
                htmlContent = lesson["htmlContent"]?.ToString(),
                slug = lesson["slug"]?.ToString(),
                estimatedMinutes = lesson["estimatedMinutes"] != null ? Convert.ToInt32(lesson["estimatedMinutes"]) : (int?)null,
                previousLessonId = lesson["previousLessonId"]?.ToString(),
                nextLessonId = lesson["nextLessonId"]?.ToString(),
                createdAt = lesson.CreatedAt,
                updatedAt = lesson.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lesson {LessonId}", id);
            return StatusCode(500, new { message = "Error fetching lesson", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lesson by slug
    /// </summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetLessonBySlug(string slug)
    {
        try
        {
            var allLessons = await _store.GetAllAsync("Lesson", 1, 1000);
            var lesson = allLessons.FirstOrDefault(l => l["slug"]?.ToString() == slug);

            if (lesson == null)
            {
                return NotFound(new { message = $"Lesson with slug '{slug}' not found" });
            }

            var result = new
            {
                id = lesson.Id.ToString(),
                topicId = lesson["topicId"]?.ToString(),
                lessonNumber = lesson["lessonNumber"] != null ? Convert.ToInt32(lesson["lessonNumber"]) : (int?)null,
                title = lesson["title"]?.ToString(),
                module = lesson["module"]?.ToString(),
                htmlContent = lesson["htmlContent"]?.ToString(),
                slug = lesson["slug"]?.ToString(),
                estimatedMinutes = lesson["estimatedMinutes"] != null ? Convert.ToInt32(lesson["estimatedMinutes"]) : (int?)null,
                previousLessonId = lesson["previousLessonId"]?.ToString(),
                nextLessonId = lesson["nextLessonId"]?.ToString(),
                createdAt = lesson.CreatedAt,
                updatedAt = lesson.UpdatedAt
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lesson by slug {Slug}", slug);
            return StatusCode(500, new { message = "Error fetching lesson", error = ex.Message });
        }
    }

    /// <summary>
    /// Get lessons by module
    /// </summary>
    [HttpGet("module/{moduleName}")]
    public async Task<IActionResult> GetLessonsByModule(string moduleName)
    {
        try
        {
            var allLessons = await _store.GetAllAsync("Lesson", 1, 1000);
            var lessons = allLessons
                .Where(l => l["module"]?.ToString()?.Contains(moduleName, StringComparison.OrdinalIgnoreCase) == true)
                .Select(lesson => new
                {
                    id = lesson.Id.ToString(),
                    lessonNumber = lesson["lessonNumber"] != null ? Convert.ToInt32(lesson["lessonNumber"]) : (int?)null,
                    title = lesson["title"]?.ToString(),
                    module = lesson["module"]?.ToString(),
                    slug = lesson["slug"]?.ToString(),
                    estimatedMinutes = lesson["estimatedMinutes"] != null ? Convert.ToInt32(lesson["estimatedMinutes"]) : (int?)null
                })
                .OrderBy(l => l.lessonNumber)
                .ToList();

            return Ok(lessons);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching lessons for module {ModuleName}", moduleName);
            return StatusCode(500, new { message = "Error fetching lessons", error = ex.Message });
        }
    }
}
