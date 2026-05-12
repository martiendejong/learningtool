using LearningTool.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SnippetsController : ControllerBase
{
    private readonly ICodeSnippetService _snippetService;

    public SnippetsController(ICodeSnippetService snippetService)
    {
        _snippetService = snippetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMySnippets([FromQuery] string? language, [FromQuery] string? search)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var snippets = await _snippetService.GetUserSnippetsAsync(userId, language, search);
        return Ok(snippets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSnippet(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var snippet = await _snippetService.GetByIdAsync(id, userId);
        if (snippet == null) return NotFound();

        return Ok(snippet);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSnippet([FromBody] CreateSnippetRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var snippet = await _snippetService.CreateAsync(
                userId,
                request.Title,
                request.Code,
                request.Language,
                request.Description,
                request.Tags,
                request.CourseId,
                request.IsPublic);

            return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSnippet(int id, [FromBody] UpdateSnippetRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        try
        {
            var snippet = await _snippetService.UpdateAsync(
                id,
                userId,
                request.Title,
                request.Code,
                request.Language,
                request.Description,
                request.Tags,
                request.IsPublic);

            if (snippet == null) return NotFound();

            return Ok(snippet);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSnippet(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var deleted = await _snippetService.DeleteAsync(id, userId);
        if (!deleted) return NotFound();

        return NoContent();
    }
}

public record CreateSnippetRequest(
    string Title,
    string Code,
    string Language,
    string? Description,
    List<string>? Tags,
    int? CourseId,
    bool IsPublic);

public record UpdateSnippetRequest(
    string Title,
    string Code,
    string Language,
    string? Description,
    List<string>? Tags,
    bool IsPublic);
