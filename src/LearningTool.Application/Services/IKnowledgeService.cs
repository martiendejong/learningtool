using LearningTool.Domain.Entities;

namespace LearningTool.Application.Services;

public interface IKnowledgeService
{
    // Skill management
    Task<Skill> AddSkillToCatalogAsync(string name, string description, DifficultyLevel difficulty);
    Task<Skill?> FindOrCreateSkillAsync(string name);
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<List<Skill>> SearchSkillsAsync(string query);
    Task<List<Skill>> GetAllSkillsAsync();

    // Topic management
    Task<Topic> AddTopicAsync(int skillId, string name, string description);
    Task<Topic?> FindOrCreateTopicAsync(int skillId, string name);
    Task<Topic?> GetTopicByIdAsync(int id);
    Task<List<Topic>> GetTopicsForSkillAsync(int skillId);

    // Course management
    Task<Course> AddCourseAsync(int topicId, string name, string description, string content,
        int estimatedMinutes, List<string> prerequisites, List<ResourceLink> resourceLinks);
    Task<Course?> GetCourseByIdAsync(int id);
    Task<List<Course>> GetCoursesForTopicAsync(int topicId);
}
