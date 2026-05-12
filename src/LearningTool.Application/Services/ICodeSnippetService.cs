using LearningTool.Domain.Entities;

namespace LearningTool.Application.Services;

public interface ICodeSnippetService
{
    Task<CodeSnippet> CreateAsync(
        string userId,
        string title,
        string code,
        string language,
        string? description,
        List<string>? tags,
        int? courseId,
        bool isPublic);

    Task<CodeSnippet?> GetByIdAsync(int id, string userId);

    Task<List<CodeSnippet>> GetUserSnippetsAsync(
        string userId,
        string? language = null,
        string? search = null);

    Task<CodeSnippet?> UpdateAsync(
        int id,
        string userId,
        string title,
        string code,
        string language,
        string? description,
        List<string>? tags,
        bool isPublic);

    Task<bool> DeleteAsync(int id, string userId);
}
