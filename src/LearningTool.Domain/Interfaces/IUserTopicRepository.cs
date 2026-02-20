using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface IUserTopicRepository
{
    Task<List<UserTopic>> GetByUserIdAsync(string userId, bool includeDeleted = false);
    Task<UserTopic?> GetByUserAndTopicAsync(string userId, int topicId);
    Task<UserTopic> CreateAsync(UserTopic userTopic);
    Task UpdateAsync(UserTopic userTopic);
    Task SoftDeleteAsync(string userId, int topicId);
}
