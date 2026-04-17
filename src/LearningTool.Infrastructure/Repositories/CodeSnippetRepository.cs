using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class CodeSnippetRepository : ICodeSnippetRepository
{
    private readonly LearningToolDbContext _context;

    public CodeSnippetRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<CodeSnippet?> GetByIdAsync(int id)
    {
        return await _context.CodeSnippets
            .Include(s => s.Course)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<CodeSnippet>> GetByUserIdAsync(string userId, string? language = null, string? search = null)
    {
        var query = _context.CodeSnippets
            .Include(s => s.Course)
            .Where(s => s.UserId == userId);

        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(s => s.Language == language);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(s =>
                EF.Functions.ILike(s.Title, term) ||
                (s.Description != null && EF.Functions.ILike(s.Description, term)));
        }

        return await query
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(string userId)
    {
        return await _context.CodeSnippets.CountAsync(s => s.UserId == userId);
    }

    public async Task<CodeSnippet> CreateAsync(CodeSnippet snippet)
    {
        _context.CodeSnippets.Add(snippet);
        await _context.SaveChangesAsync();
        return snippet;
    }

    public async Task UpdateAsync(CodeSnippet snippet)
    {
        snippet.UpdatedAt = DateTime.UtcNow;
        _context.CodeSnippets.Update(snippet);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id, string userId)
    {
        var snippet = await _context.CodeSnippets
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

        if (snippet != null)
        {
            snippet.IsDeleted = true;
            snippet.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
