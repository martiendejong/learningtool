using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "SYSTEMADMIN")]
public class AdminController : ControllerBase
{
    private readonly LearningToolDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(LearningToolDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET /api/admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalOrganizations = await _context.Organizations.CountAsync();
        var totalSkills = await _context.Skills.CountAsync();
        var totalTopics = await _context.Topics.CountAsync();
        var totalCourses = await _context.Courses.CountAsync();
        var totalCompletions = await _context.UserCourses
            .CountAsync(uc => uc.Status == UserCourseStatus.Completed);
        var totalInProgress = await _context.UserCourses
            .CountAsync(uc => uc.Status == UserCourseStatus.InProgress);

        return Ok(new
        {
            totalUsers,
            totalOrganizations,
            totalSkills,
            totalTopics,
            totalCourses,
            totalCompletions,
            totalInProgress
        });
    }

    // ── Courses ────────────────────────────────────────────────────────────────

    // GET /api/admin/courses
    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var courses = await _context.Courses
            .Include(c => c.Topic)
                .ThenInclude(t => t.Skill)
            .OrderBy(c => c.Topic.Skill.Name)
            .ThenBy(c => c.Topic.Name)
            .ThenBy(c => c.Name)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.EstimatedMinutes,
                c.CreatedAt,
                c.ContentGeneratedAt,
                topic = new { c.Topic.Id, c.Topic.Name },
                skill = new { c.Topic.Skill.Id, c.Topic.Skill.Name }
            })
            .ToListAsync();

        return Ok(courses);
    }

    // PUT /api/admin/courses/{id}
    [HttpPut("courses/{id}")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] AdminUpdateCourseRequest request)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound(new { message = "Course not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            course.Name = request.Name;

        if (request.Description != null)
            course.Description = request.Description;

        if (request.EstimatedMinutes.HasValue)
            course.EstimatedMinutes = request.EstimatedMinutes.Value;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Course updated", course.Id, course.Name });
    }

    // DELETE /api/admin/courses/{id}
    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return NotFound(new { message = "Course not found" });

        course.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Course deleted" });
    }

    // ── Skills ─────────────────────────────────────────────────────────────────

    // GET /api/admin/skills
    [HttpGet("skills")]
    public async Task<IActionResult> GetSkills()
    {
        var skills = await _context.Skills
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Description,
                s.Difficulty,
                s.CreatedAt,
                topicCount = s.Topics.Count(t => !t.IsDeleted),
                courseCount = s.Topics
                    .Where(t => !t.IsDeleted)
                    .SelectMany(t => t.Courses)
                    .Count(c => !c.IsDeleted)
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        return Ok(skills);
    }

    // PUT /api/admin/skills/{id}
    [HttpPut("skills/{id}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] AdminUpdateSkillRequest request)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
        if (skill == null) return NotFound(new { message = "Skill not found" });

        if (!string.IsNullOrWhiteSpace(request.Name))
            skill.Name = request.Name;

        if (request.Description != null)
            skill.Description = request.Description;

        if (request.Difficulty.HasValue)
            skill.Difficulty = request.Difficulty.Value;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Skill updated", skill.Id, skill.Name });
    }

    // DELETE /api/admin/skills/{id}
    [HttpDelete("skills/{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var skill = await _context.Skills.FirstOrDefaultAsync(s => s.Id == id);
        if (skill == null) return NotFound(new { message = "Skill not found" });

        var hasCourses = await _context.Topics
            .Where(t => t.SkillId == id)
            .AnyAsync(t => t.Courses.Any(c => !c.IsDeleted));

        if (hasCourses)
            return BadRequest(new { message = "Cannot delete skill with active courses. Delete or reassign courses first." });

        skill.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Skill deleted" });
    }

    // ── Topics ─────────────────────────────────────────────────────────────────

    // GET /api/admin/topics
    [HttpGet("topics")]
    public async Task<IActionResult> GetTopics()
    {
        var topics = await _context.Topics
            .Include(t => t.Skill)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.CreatedAt,
                skill = new { t.Skill.Id, t.Skill.Name },
                courseCount = t.Courses.Count(c => !c.IsDeleted)
            })
            .OrderBy(t => t.skill.Name)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return Ok(topics);
    }

    // DELETE /api/admin/topics/{id}
    [HttpDelete("topics/{id}")]
    public async Task<IActionResult> DeleteTopic(int id)
    {
        var topic = await _context.Topics.FirstOrDefaultAsync(t => t.Id == id);
        if (topic == null) return NotFound(new { message = "Topic not found" });

        var hasCourses = await _context.Courses.AnyAsync(c => c.TopicId == id);
        if (hasCourses)
            return BadRequest(new { message = "Cannot delete topic with active courses. Delete courses first." });

        topic.IsDeleted = true;
        await _context.SaveChangesAsync();
        return Ok(new { message = "Topic deleted" });
    }
}

public record AdminUpdateCourseRequest(
    string? Name = null,
    string? Description = null,
    int? EstimatedMinutes = null);

public record AdminUpdateSkillRequest(
    string? Name = null,
    string? Description = null,
    DifficultyLevel? Difficulty = null);
