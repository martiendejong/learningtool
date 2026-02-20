using LearningTool.Domain.Entities;

namespace LearningTool.Application.Services;

public interface IUserLearningService
{
    // User skill management
    Task<UserSkill> AddSkillToUserAsync(string userId, int skillId);
    Task RemoveSkillFromUserAsync(string userId, int skillId);
    Task<List<UserSkill>> GetUserSkillsAsync(string userId, bool includeDeleted = false);

    // User topic management
    Task<UserTopic> AddTopicToUserAsync(string userId, int topicId);
    Task RemoveTopicFromUserAsync(string userId, int topicId);
    Task<List<UserTopic>> GetUserTopicsAsync(string userId, bool includeDeleted = false);

    // User course management
    Task<UserCourse> StartCourseAsync(string userId, int courseId);
    Task<UserCourse> CompleteCourseAsync(string userId, int courseId, int assessmentScore);
    Task<List<UserCourse>> GetCompletedCoursesAsync(string userId);
    Task<List<UserCourse>> GetInProgressCoursesAsync(string userId);
    Task<UserCourse?> GetUserCourseAsync(string userId, int courseId);
}
