using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;

namespace LearningTool.Application.Services;

public class UserLearningService : IUserLearningService
{
    private readonly IUserSkillRepository _userSkillRepository;
    private readonly IUserTopicRepository _userTopicRepository;
    private readonly IUserCourseRepository _userCourseRepository;

    public UserLearningService(
        IUserSkillRepository userSkillRepository,
        IUserTopicRepository userTopicRepository,
        IUserCourseRepository userCourseRepository)
    {
        _userSkillRepository = userSkillRepository;
        _userTopicRepository = userTopicRepository;
        _userCourseRepository = userCourseRepository;
    }

    public async Task<UserSkill> AddSkillToUserAsync(string userId, int skillId)
    {
        var existing = await _userSkillRepository.GetByUserAndSkillAsync(userId, skillId);
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                await _userSkillRepository.UpdateAsync(existing);
            }
            return existing;
        }

        var userSkill = new UserSkill
        {
            UserId = userId,
            SkillId = skillId,
            Status = UserSkillStatus.WantToLearn
        };

        return await _userSkillRepository.CreateAsync(userSkill);
    }

    public async Task RemoveSkillFromUserAsync(string userId, int skillId)
    {
        await _userSkillRepository.SoftDeleteAsync(userId, skillId);
    }

    public async Task<List<UserSkill>> GetUserSkillsAsync(string userId, bool includeDeleted = false)
    {
        return await _userSkillRepository.GetByUserIdAsync(userId, includeDeleted);
    }

    public async Task<UserTopic> AddTopicToUserAsync(string userId, int topicId)
    {
        var existing = await _userTopicRepository.GetByUserAndTopicAsync(userId, topicId);
        if (existing != null)
        {
            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                await _userTopicRepository.UpdateAsync(existing);
            }
            return existing;
        }

        var userTopic = new UserTopic
        {
            UserId = userId,
            TopicId = topicId,
            Status = UserTopicStatus.NotStarted
        };

        return await _userTopicRepository.CreateAsync(userTopic);
    }

    public async Task RemoveTopicFromUserAsync(string userId, int topicId)
    {
        await _userTopicRepository.SoftDeleteAsync(userId, topicId);
    }

    public async Task<List<UserTopic>> GetUserTopicsAsync(string userId, bool includeDeleted = false)
    {
        return await _userTopicRepository.GetByUserIdAsync(userId, includeDeleted);
    }

    public async Task<UserCourse> StartCourseAsync(string userId, int courseId)
    {
        var existing = await _userCourseRepository.GetByUserAndCourseAsync(userId, courseId);
        if (existing != null)
        {
            if (existing.Status == UserCourseStatus.NotStarted)
            {
                existing.Status = UserCourseStatus.InProgress;
                existing.StartedAt = DateTime.UtcNow;
                await _userCourseRepository.UpdateAsync(existing);
            }
            return existing;
        }

        var userCourse = new UserCourse
        {
            UserId = userId,
            CourseId = courseId,
            Status = UserCourseStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        return await _userCourseRepository.CreateAsync(userCourse);
    }

    public async Task<UserCourse> CompleteCourseAsync(string userId, int courseId, int assessmentScore)
    {
        var userCourse = await _userCourseRepository.GetByUserAndCourseAsync(userId, courseId);
        if (userCourse == null)
        {
            throw new InvalidOperationException("Course not started");
        }

        userCourse.Status = UserCourseStatus.Completed;
        userCourse.CompletedAt = DateTime.UtcNow;
        userCourse.AssessmentScore = assessmentScore;

        if (userCourse.StartedAt.HasValue)
        {
            var duration = (int)(DateTime.UtcNow - userCourse.StartedAt.Value).TotalMinutes;
            userCourse.MinutesSpent = duration;
        }

        await _userCourseRepository.UpdateAsync(userCourse);
        return userCourse;
    }

    public async Task<List<UserCourse>> GetCompletedCoursesAsync(string userId)
    {
        return await _userCourseRepository.GetCompletedByUserIdAsync(userId);
    }

    public async Task<List<UserCourse>> GetInProgressCoursesAsync(string userId)
    {
        return await _userCourseRepository.GetInProgressByUserIdAsync(userId);
    }

    public async Task<UserCourse?> GetUserCourseAsync(string userId, int courseId)
    {
        return await _userCourseRepository.GetByUserAndCourseAsync(userId, courseId);
    }
}
