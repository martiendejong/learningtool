using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class CourseRepository : ICourseRepository
{
    private readonly LearningToolDbContext _context;

    public CourseRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<Course?> GetByIdAsync(int id)
    {
        return await _context.Courses
            .Include(c => c.Topic)
                .ThenInclude(t => t.Skill)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Course>> GetByTopicIdAsync(int topicId, bool includeDeleted = false)
    {
        var query = _context.Courses
            .Where(c => c.TopicId == topicId);

        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Course> CreateAsync(Course course)
    {
        course.CreatedAt = DateTime.UtcNow;
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task UpdateAsync(Course course)
    {
        _context.Courses.Update(course);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int id)
    {
        var course = await _context.Courses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course != null)
        {
            course.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
    }
}
