using LearningTool.Application.Services;
using LearningTool.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SkillsController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;

    public SkillsController(
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService)
    {
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog([FromQuery] string? search = null)
    {
        var skills = string.IsNullOrWhiteSpace(search)
            ? await _knowledgeService.GetAllSkillsAsync()
            : await _knowledgeService.SearchSkillsAsync(search);

        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkillById(int id)
    {
        var skill = await _knowledgeService.GetSkillByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }
        return Ok(skill);
    }

    [HttpGet("my-skills")]
    public async Task<IActionResult> GetMySkills()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        var userSkills = await _userLearningService.GetUserSkillsAsync(userId);
        return Ok(userSkills);
    }

    [HttpPost]
    public async Task<IActionResult> AddSkillToMyList([FromBody] AddSkillRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        // Find or create skill in catalog
        var skill = await _knowledgeService.FindOrCreateSkillAsync(request.SkillName);

        // Add to user's list
        var userSkill = await _userLearningService.AddSkillToUserAsync(userId, skill.Id);

        return CreatedAtAction(nameof(GetMySkills), new { id = userSkill.Id }, userSkill);
    }

    [HttpDelete("{skillId}")]
    public async Task<IActionResult> RemoveSkillFromMyList(int skillId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        await _userLearningService.RemoveSkillFromUserAsync(userId, skillId);
        return NoContent();
    }
}

public record AddSkillRequest(string SkillName);
