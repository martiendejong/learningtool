using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface IUserCourseRepository
{
    Task<List<UserCourse>> GetByUserIdAsync(string userId, bool includeDeleted = false);
    Task<List<UserCourse>> GetCompletedByUserIdAsync(string userId);
    Task<List<UserCourse>> GetInProgressByUserIdAsync(string userId);
    Task<UserCourse?> GetByUserAndCourseAsync(string userId, int courseId);
    Task<UserCourse> CreateAsync(UserCourse userCourse);
    Task UpdateAsync(UserCourse userCourse);
    Task SoftDeleteAsync(string userId, int courseId);
}
