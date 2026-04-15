using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/bookmarks")]
[Authorize]
public class BookmarkController : ControllerBase
{
    private readonly LearningToolDbContext _context;

    public BookmarkController(LearningToolDbContext context)
    {
        _context = context;
    }

    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    // GET /api/bookmarks
    [HttpGet]
    public async Task<IActionResult> GetBookmarks()
    {
        var userId = UserId;

        var bookmarks = await _context.Bookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Course)
                .ThenInclude(c => c.Topic)
                    .ThenInclude(t => t.Skill)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new
            {
                b.Id,
                b.Note,
                b.CreatedAt,
                b.UpdatedAt,
                course = new
                {
                    b.Course.Id,
                    b.Course.Name,
                    b.Course.Description,
                    b.Course.EstimatedMinutes
                },
                topic = new { b.Course.Topic.Id, b.Course.Topic.Name },
                skill = new { b.Course.Topic.Skill.Id, b.Course.Topic.Skill.Name }
            })
            .ToListAsync();

        return Ok(bookmarks);
    }

    // GET /api/bookmarks/check/{courseId}
    [HttpGet("check/{courseId:int}")]
    public async Task<IActionResult> CheckBookmark(int courseId)
    {
        var userId = UserId;
        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CourseId == courseId);

        return Ok(new { isBookmarked = bookmark != null, note = bookmark?.Note });
    }

    // POST /api/bookmarks/{courseId}  — toggle on/off
    [HttpPost("{courseId:int}")]
    public async Task<IActionResult> ToggleBookmark(int courseId)
    {
        var userId = UserId;

        var existing = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CourseId == courseId);

        if (existing != null)
        {
            _context.Bookmarks.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok(new { isBookmarked = false });
        }

        // Verify course exists
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
        if (!courseExists) return NotFound(new { message = "Course not found" });

        _context.Bookmarks.Add(new Bookmark
        {
            UserId = userId,
            CourseId = courseId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new { isBookmarked = true });
    }

    // PUT /api/bookmarks/{courseId}/note
    [HttpPut("{courseId:int}/note")]
    public async Task<IActionResult> UpdateNote(int courseId, [FromBody] UpdateBookmarkNoteRequest request)
    {
        var userId = UserId;

        var bookmark = await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.CourseId == courseId);

        if (bookmark == null) return NotFound(new { message = "Bookmark not found" });

        bookmark.Note = request.Note;
        bookmark.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(new { note = bookmark.Note });
    }
}

public record UpdateBookmarkNoteRequest(string? Note);
