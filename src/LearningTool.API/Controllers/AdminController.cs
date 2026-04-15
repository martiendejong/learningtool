using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

// ── Bundle management (SYSTEMADMIN) ───────────────────────────────────────────

[ApiController]
[Route("api/admin/bundles")]
[Authorize(Roles = "SYSTEMADMIN")]
public class AdminBundleController : ControllerBase
{
    private readonly LearningToolDbContext _context;

    public AdminBundleController(LearningToolDbContext context)
    {
        _context = context;
    }

    // GET /api/admin/bundles
    [HttpGet]
    public async Task<IActionResult> GetBundles()
    {
        var bundles = await _context.Bundles
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.Description,
                b.CreatedAt,
                skillCount = b.BundleSkills.Count,
                orgCount = b.OrganizationBundles.Count,
                skills = b.BundleSkills.Select(bs => new { bs.Skill.Id, bs.Skill.Name })
            })
            .OrderBy(b => b.Name)
            .ToListAsync();

        return Ok(bundles);
    }

    // POST /api/admin/bundles
    [HttpPost]
    public async Task<IActionResult> CreateBundle([FromBody] BundleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Name is required" });

        if (await _context.Bundles.AnyAsync(b => b.Name == request.Name))
            return BadRequest(new { message = "A bundle with this name already exists" });

        var bundle = new Bundle { Name = request.Name, Description = request.Description };
        _context.Bundles.Add(bundle);
        await _context.SaveChangesAsync();

        return Ok(new { bundle.Id, bundle.Name, bundle.Description, bundle.CreatedAt });
    }

    // PUT /api/admin/bundles/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBundle(int id, [FromBody] BundleRequest request)
    {
        var bundle = await _context.Bundles.FindAsync(id);
        if (bundle == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name)) bundle.Name = request.Name;
        if (request.Description != null) bundle.Description = request.Description;

        await _context.SaveChangesAsync();
        return Ok(new { bundle.Id, bundle.Name, bundle.Description });
    }

    // DELETE /api/admin/bundles/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteBundle(int id)
    {
        var bundle = await _context.Bundles.FindAsync(id);
        if (bundle == null) return NotFound();

        _context.Bundles.Remove(bundle);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/admin/bundles/{bundleId}/skills/{skillId}
    [HttpPost("{bundleId:int}/skills/{skillId:int}")]
    public async Task<IActionResult> AddSkillToBundle(int bundleId, int skillId)
    {
        if (!await _context.Bundles.AnyAsync(b => b.Id == bundleId))
            return NotFound(new { message = "Bundle not found" });
        if (!await _context.Skills.AnyAsync(s => s.Id == skillId))
            return NotFound(new { message = "Skill not found" });
        if (await _context.BundleSkills.AnyAsync(bs => bs.BundleId == bundleId && bs.SkillId == skillId))
            return BadRequest(new { message = "Skill already in bundle" });

        _context.BundleSkills.Add(new BundleSkill { BundleId = bundleId, SkillId = skillId });
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/admin/bundles/{bundleId}/skills/{skillId}
    [HttpDelete("{bundleId:int}/skills/{skillId:int}")]
    public async Task<IActionResult> RemoveSkillFromBundle(int bundleId, int skillId)
    {
        var bs = await _context.BundleSkills
            .FirstOrDefaultAsync(x => x.BundleId == bundleId && x.SkillId == skillId);
        if (bs == null) return NotFound();

        _context.BundleSkills.Remove(bs);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/admin/bundles/{bundleId}/organizations/{orgId}
    [HttpPost("{bundleId:int}/organizations/{orgId:int}")]
    public async Task<IActionResult> AssignBundleToOrg(
        int bundleId, int orgId, [FromBody] AssignBundleToOrgRequest request)
    {
        if (!await _context.Bundles.AnyAsync(b => b.Id == bundleId))
            return NotFound(new { message = "Bundle not found" });
        if (!await _context.Organizations.AnyAsync(o => o.Id == orgId))
            return NotFound(new { message = "Organization not found" });
        if (await _context.OrganizationBundles.AnyAsync(ob => ob.BundleId == bundleId && ob.OrganizationId == orgId))
            return BadRequest(new { message = "Bundle already assigned to this organization" });

        _context.OrganizationBundles.Add(new OrganizationBundle
        {
            BundleId = bundleId,
            OrganizationId = orgId,
            MaxUsers = request.MaxUsers ?? 0,
            IsUnlimited = request.IsUnlimited ?? false
        });
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/admin/bundles/{bundleId}/organizations/{orgId}  — update seat config
    [HttpPut("{bundleId:int}/organizations/{orgId:int}")]
    public async Task<IActionResult> UpdateOrgBundle(
        int bundleId, int orgId, [FromBody] AssignBundleToOrgRequest request)
    {
        var ob = await _context.OrganizationBundles
            .FirstOrDefaultAsync(x => x.BundleId == bundleId && x.OrganizationId == orgId);
        if (ob == null) return NotFound();

        if (request.MaxUsers.HasValue) ob.MaxUsers = request.MaxUsers.Value;
        if (request.IsUnlimited.HasValue) ob.IsUnlimited = request.IsUnlimited.Value;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/admin/bundles/{bundleId}/organizations/{orgId}
    [HttpDelete("{bundleId:int}/organizations/{orgId:int}")]
    public async Task<IActionResult> UnassignBundleFromOrg(int bundleId, int orgId)
    {
        var ob = await _context.OrganizationBundles
            .FirstOrDefaultAsync(x => x.BundleId == bundleId && x.OrganizationId == orgId);
        if (ob == null) return NotFound();

        _context.OrganizationBundles.Remove(ob);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}

public record BundleRequest(string Name, string? Description = null);
public record AssignBundleToOrgRequest(int? MaxUsers = null, bool? IsUnlimited = null);
