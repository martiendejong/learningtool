using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly LearningToolDbContext _context;

    public TopicRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<Topic?> GetByIdAsync(int id)
    {
        return await _context.Topics
            .Include(t => t.Skill)
            .Include(t => t.Courses)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Topic?> GetByNameAsync(string name)
    {
        return await _context.Topics
            .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());
    }

    public async Task<List<Topic>> GetBySkillIdAsync(int skillId, bool includeDeleted = false)
    {
        var query = _context.Topics
            .Include(t => t.Courses)
            .Where(t => t.SkillId == skillId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Topic> CreateAsync(Topic topic)
    {
        topic.CreatedAt = DateTime.UtcNow;
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task UpdateAsync(Topic topic)
    {
        _context.Topics.Update(topic);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var topic = await _context.Topics
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (topic != null)
        {
            topic.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
