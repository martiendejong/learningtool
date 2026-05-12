using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;

namespace LearningTool.Application.Services;

public class CodeSnippetService : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _repository;

    public CodeSnippetService(ICodeSnippetRepository repository)
    {
        _repository = repository;
    }

    public async Task<CodeSnippet> CreateAsync(
        string userId,
        string title,
        string code,
        string language,
        string? description,
        List<string>? tags,
        int? courseId,
        bool isPublic)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        var snippet = new CodeSnippet
        {
            UserId = userId,
            Title = title.Trim(),
            Code = code,
            Language = string.IsNullOrWhiteSpace(language) ? "plaintext" : language.Trim().ToLowerInvariant(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Tags = NormalizeTags(tags),
            CourseId = courseId,
            IsPublic = isPublic,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(snippet);
    }

    public async Task<CodeSnippet?> GetByIdAsync(int id, string userId)
    {
        var snippet = await _repository.GetByIdAsync(id);
        if (snippet == null) return null;

        // Owner can always view; others only if public
        if (snippet.UserId != userId && !snippet.IsPublic) return null;

        return snippet;
    }

    public Task<List<CodeSnippet>> GetUserSnippetsAsync(
        string userId,
        string? language = null,
        string? search = null)
    {
        return _repository.GetByUserIdAsync(userId, language, search);
    }

    public async Task<CodeSnippet?> UpdateAsync(
        int id,
        string userId,
        string title,
        string code,
        string language,
        string? description,
        List<string>? tags,
        bool isPublic)
    {
        var snippet = await _repository.GetByIdAsync(id);
        if (snippet == null || snippet.UserId != userId) return null;

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required", nameof(title));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required", nameof(code));

        snippet.Title = title.Trim();
        snippet.Code = code;
        snippet.Language = string.IsNullOrWhiteSpace(language) ? "plaintext" : language.Trim().ToLowerInvariant();
        snippet.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        snippet.Tags = NormalizeTags(tags);
        snippet.IsPublic = isPublic;

        await _repository.UpdateAsync(snippet);
        return snippet;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var snippet = await _repository.GetByIdAsync(id);
        if (snippet == null || snippet.UserId != userId) return false;

        await _repository.SoftDeleteAsync(id, userId);
        return true;
    }

    private static List<string> NormalizeTags(List<string>? tags)
    {
        if (tags == null) return new List<string>();
        return tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct()
            .Take(10)
            .ToList();
    }
}
