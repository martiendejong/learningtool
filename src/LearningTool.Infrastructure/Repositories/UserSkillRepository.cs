using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class UserSkillRepository : IUserSkillRepository
{
    private readonly LearningToolDbContext _context;

    public UserSkillRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserSkill>> GetByUserIdAsync(string userId, bool includeDeleted = false)
    {
        var query = _context.UserSkills
            .Include(us => us.Skill)
                .ThenInclude(s => s.Topics)
                    .ThenInclude(t => t.Courses)
            .Where(us => us.UserId == userId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderByDescending(us => us.AddedAt)
            .ToListAsync();
    }

    public async Task<UserSkill?> GetByUserAndSkillAsync(string userId, int skillId)
    {
        return await _context.UserSkills
            .Include(us => us.Skill)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);
    }

    public async Task<UserSkill> CreateAsync(UserSkill userSkill)
    {
        userSkill.AddedAt = DateTime.UtcNow;
        _context.UserSkills.Add(userSkill);
        await _context.SaveChangesAsync();
        return userSkill;
    }

    public async Task UpdateAsync(UserSkill userSkill)
    {
        _context.UserSkills.Update(userSkill);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(string userId, int skillId)
    {
        var userSkill = await _context.UserSkills
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (userSkill != null)
        {
            userSkill.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
