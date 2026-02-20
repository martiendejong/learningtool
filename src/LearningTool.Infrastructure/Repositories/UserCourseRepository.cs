using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class UserCourseRepository : IUserCourseRepository
{
    private readonly LearningToolDbContext _context;

    public UserCourseRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserCourse>> GetByUserIdAsync(string userId, bool includeDeleted = false)
    {
        var query = _context.UserCourses
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Topic)
                    .ThenInclude(t => t.Skill)
            .Where(uc => uc.UserId == userId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderByDescending(uc => uc.StartedAt)
            .ToListAsync();
    }

    public async Task<List<UserCourse>> GetCompletedByUserIdAsync(string userId)
    {
        return await _context.UserCourses
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Topic)
                    .ThenInclude(t => t.Skill)
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.Completed)
            .OrderByDescending(uc => uc.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<UserCourse>> GetInProgressByUserIdAsync(string userId)
    {
        return await _context.UserCourses
            .Include(uc => uc.Course)
                .ThenInclude(c => c.Topic)
                    .ThenInclude(t => t.Skill)
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.InProgress)
            .OrderByDescending(uc => uc.StartedAt)
            .ToListAsync();
    }

    public async Task<UserCourse?> GetByUserAndCourseAsync(string userId, int courseId)
    {
        return await _context.UserCourses
            .Include(uc => uc.Course)
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);
    }

    public async Task<UserCourse> CreateAsync(UserCourse userCourse)
    {
        _context.UserCourses.Add(userCourse);
        await _context.SaveChangesAsync();
        return userCourse;
    }

    public async Task UpdateAsync(UserCourse userCourse)
    {
        _context.UserCourses.Update(userCourse);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(string userId, int courseId)
    {
        var userCourse = await _context.UserCourses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CourseId == courseId);

        if (userCourse != null)
        {
            userCourse.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
