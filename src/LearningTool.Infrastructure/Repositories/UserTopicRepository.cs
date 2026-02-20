using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class UserTopicRepository : IUserTopicRepository
{
    private readonly LearningToolDbContext _context;

    public UserTopicRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserTopic>> GetByUserIdAsync(string userId, bool includeDeleted = false)
    {
        var query = _context.UserTopics
            .Include(ut => ut.Topic)
                .ThenInclude(t => t.Skill)
            .Where(ut => ut.UserId == userId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderByDescending(ut => ut.AddedAt)
            .ToListAsync();
    }

    public async Task<UserTopic?> GetByUserAndTopicAsync(string userId, int topicId)
    {
        return await _context.UserTopics
            .Include(ut => ut.Topic)
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TopicId == topicId);
    }

    public async Task<UserTopic> CreateAsync(UserTopic userTopic)
    {
        userTopic.AddedAt = DateTime.UtcNow;
        _context.UserTopics.Add(userTopic);
        await _context.SaveChangesAsync();
        return userTopic;
    }

    public async Task UpdateAsync(UserTopic userTopic)
    {
        _context.UserTopics.Update(userTopic);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(string userId, int topicId)
    {
        var userTopic = await _context.UserTopics
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ut => ut.UserId == userId && ut.TopicId == topicId);

        if (userTopic != null)
        {
            userTopic.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
