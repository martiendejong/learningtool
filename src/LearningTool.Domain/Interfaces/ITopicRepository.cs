using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(int id);
    Task<Topic?> GetByNameAsync(string name);
    Task<List<Topic>> GetBySkillIdAsync(int skillId, bool includeDeleted = false);
    Task<Topic> CreateAsync(Topic topic);
    Task UpdateAsync(Topic topic);
    Task SoftDeleteAsync(int id);
}
