using LearningTool.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TopicsController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IUserLearningService _userLearningService;

    public TopicsController(
        IKnowledgeService knowledgeService,
        IUserLearningService userLearningService)
    {
        _knowledgeService = knowledgeService;
        _userLearningService = userLearningService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTopicById(int id)
    {
        var topic = await _knowledgeService.GetTopicByIdAsync(id);
        if (topic == null)
        {
            return NotFound();
        }
        return Ok(topic);
    }

    [HttpGet("skill/{skillId}")]
    public async Task<IActionResult> GetTopicsForSkill(int skillId)
    {
        var topics = await _knowledgeService.GetTopicsForSkillAsync(skillId);
        return Ok(topics);
    }

    [HttpGet("my-topics")]
    public async Task<IActionResult> GetMyTopics()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var userTopics = await _userLearningService.GetUserTopicsAsync(userId);
        return Ok(userTopics);
    }

    [HttpPost]
    public async Task<IActionResult> AddTopicToMyList([FromBody] AddTopicRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var userTopic = await _userLearningService.AddTopicToUserAsync(userId, request.TopicId);
        return CreatedAtAction(nameof(GetMyTopics), new { id = userTopic.Id }, userTopic);
    }

    [HttpDelete("{topicId}")]
    public async Task<IActionResult> RemoveTopicFromMyList(int topicId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        await _userLearningService.RemoveTopicFromUserAsync(userId, topicId);
        return NoContent();
    }
}

public record AddTopicRequest(int TopicId);
