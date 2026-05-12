using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface ICodeSnippetRepository
{
    Task<CodeSnippet?> GetByIdAsync(int id);
    Task<List<CodeSnippet>> GetByUserIdAsync(string userId, string? language = null, string? search = null);
    Task<int> CountByUserIdAsync(string userId);
    Task<CodeSnippet> CreateAsync(CodeSnippet snippet);
    Task UpdateAsync(CodeSnippet snippet);
    Task SoftDeleteAsync(int id, string userId);
}
