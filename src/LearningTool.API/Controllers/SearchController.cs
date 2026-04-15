using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly LearningToolDbContext _context;

    public SearchController(LearningToolDbContext context)
    {
        _context = context;
    }

    // GET /api/search?q=xxx
    // Returns up to 5 skills, 5 topics, and 10 courses matching the query.
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return BadRequest(new { message = "Search query must be at least 2 characters" });

        var term = q.Trim().ToLower();

        var skills = await _context.Skills
            .Where(s => EF.Functions.ILike(s.Name, $"%{term}%")
                     || EF.Functions.ILike(s.Description, $"%{term}%"))
            .Select(s => new { s.Id, s.Name, s.Description, s.Difficulty })
            .Take(5)
            .ToListAsync();

        var topics = await _context.Topics
            .Include(t => t.Skill)
            .Where(t => EF.Functions.ILike(t.Name, $"%{term}%")
                     || EF.Functions.ILike(t.Description, $"%{term}%"))
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                skill = new { t.Skill.Id, t.Skill.Name }
            })
            .Take(5)
            .ToListAsync();

        var courses = await _context.Courses
            .Include(c => c.Topic)
                .ThenInclude(t => t.Skill)
            .Where(c => EF.Functions.ILike(c.Name, $"%{term}%")
                     || EF.Functions.ILike(c.Description, $"%{term}%"))
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.EstimatedMinutes,
                topic = new { c.Topic.Id, c.Topic.Name },
                skill = new { c.Topic.Skill.Id, c.Topic.Skill.Name }
            })
            .Take(10)
            .ToListAsync();

        return Ok(new { skills, topics, courses });
    }
}
