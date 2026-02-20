using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int id);
    Task<List<Course>> GetByTopicIdAsync(int topicId, bool includeDeleted = false);
    Task<Course> CreateAsync(Course course);
    Task UpdateAsync(Course course);
    Task SoftDeleteAsync(int id);
}
