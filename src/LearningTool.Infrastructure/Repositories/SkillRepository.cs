using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class SkillRepository : ISkillRepository
{
    private readonly LearningToolDbContext _context;

    public SkillRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        return await _context.Skills
            .Include(s => s.Topics)
                .ThenInclude(t => t.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill?> GetByNameAsync(string name)
    {
        return await _context.Skills
            .FirstOrDefaultAsync(s => s.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Skill>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.Skills.AsQueryable();

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .Include(s => s.Topics)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<Skill>> SearchAsync(string query, bool includeDeleted = false)
    {
        var skillsQuery = _context.Skills.AsQueryable();

        if (includeDeleted)
        {
            skillsQuery = skillsQuery.IgnoreQueryFilters();
        }

        return await skillsQuery
            .Where(s => EF.Functions.Like(s.Name, $"%{query}%") ||
                        EF.Functions.Like(s.Description, $"%{query}%"))
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Skill> CreateAsync(Skill skill)
    {
        skill.CreatedAt = DateTime.UtcNow;
        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return skill;
    }

    public async Task UpdateAsync(Skill skill)
    {
        _context.Skills.Update(skill);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var skill = await _context.Skills
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill != null)
        {
            skill.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
