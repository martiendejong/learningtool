using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Hazina.API.Generic.Dynamic;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly DynamicEntityStore _store;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(
        DynamicEntityStore store,
        ILogger<SkillsController> logger)
    {
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Get current user's skills (filtered by userId)
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMySkills()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        // Get all UserSkills
        var allUserSkills = await _store.GetAllAsync("UserSkill", 1, 1000);

        // Filter by current user
        var myUserSkills = allUserSkills
            .Where(us => us["userId"]?.ToString() == userId)
            .ToList();

        // Get all skills to populate skill details
        var allSkills = await _store.GetAllAsync("Skill", 1, 1000);
        var skillsMap = allSkills.ToDictionary(s => s.Id, s => s);

        // Map UserSkills with Skill details
        var result = myUserSkills.Select(us =>
        {
            var skillIdStr = us["skillId"]?.ToString();
            DynamicEntity? skillEntity = null;

            if (Guid.TryParse(skillIdStr, out var skillGuid))
            {
                skillsMap.TryGetValue(skillGuid, out skillEntity);
            }

            return new
            {
                id = us.Id,
                skillId = skillIdStr,
                userId = us["userId"]?.ToString(),
                status = us["status"]?.ToString(),
                startedAt = us["startedAt"]?.ToString(),
                createdAt = us.CreatedAt,
                skill = skillEntity != null
                    ? new
                    {
                        id = skillEntity.Id,
                        name = skillEntity["name"]?.ToString(),
                        description = skillEntity["description"]?.ToString(),
                        difficulty = skillEntity["difficulty"]?.ToString()
                    }
                    : null
            };
        }).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Add a skill to current user
    /// </summary>
    [HttpPost("my")]
    public async Task<IActionResult> AddSkillToUser([FromBody] AddSkillRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (string.IsNullOrEmpty(request.SkillId))
        {
            return BadRequest(new { message = "SkillId is required" });
        }

        // Check if skill exists
        if (!Guid.TryParse(request.SkillId, out var skillGuid))
        {
            return BadRequest(new { message = "Invalid skill ID format" });
        }

        var skill = await _store.GetByIdAsync("Skill", skillGuid);
        if (skill == null)
        {
            return NotFound(new { message = "Skill not found" });
        }

        // Check if user already has this skill
        var allUserSkills = await _store.GetAllAsync("UserSkill", 1, 1000);
        var existing = allUserSkills.FirstOrDefault(us =>
            us["userId"]?.ToString() == userId &&
            us["skillId"]?.ToString() == request.SkillId);

        if (existing != null)
        {
            return BadRequest(new { message = "User already has this skill" });
        }

        // Create UserSkill
        var userSkill = new DynamicEntity();
        userSkill["userId"] = userId;
        userSkill["skillId"] = skillGuid;
        userSkill["status"] = request.Status ?? "InProgress";
        userSkill["startedAt"] = DateTime.UtcNow.ToString("O");

        await _store.CreateAsync("UserSkill", userSkill);

        return Ok(new
        {
            id = userSkill.Id,
            skillId = request.SkillId,
            userId = userId,
            status = userSkill["status"],
            startedAt = userSkill["startedAt"],
            skill = new
            {
                id = skill.Id,
                name = skill["name"]?.ToString(),
                description = skill["description"]?.ToString(),
                difficulty = skill["difficulty"]?.ToString()
            }
        });
    }

    /// <summary>
    /// Remove a skill from current user
    /// </summary>
    [HttpDelete("my/{userSkillId}")]
    public async Task<IActionResult> RemoveSkillFromUser(string userSkillId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        if (!Guid.TryParse(userSkillId, out var userSkillGuid))
        {
            return BadRequest(new { message = "Invalid UserSkill ID format" });
        }

        var userSkill = await _store.GetByIdAsync("UserSkill", userSkillGuid);
        if (userSkill == null)
        {
            return NotFound(new { message = "UserSkill not found" });
        }

        // Verify ownership
        if (userSkill["userId"]?.ToString() != userId)
        {
            return Forbid();
        }

        await _store.DeleteAsync("UserSkill", userSkillGuid);
        return Ok(new { message = "Skill removed successfully" });
    }
}

public record AddSkillRequest(string SkillId, string? Status);
