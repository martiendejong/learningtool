using LearningTool.Domain.Entities;
using LearningTool.Domain.Interfaces;
using LearningTool.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.Infrastructure.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly LearningToolDbContext _context;

    public ChatMessageRepository(LearningToolDbContext context)
    {
        _context = context;
    }

    public async Task<List<ChatMessage>> GetByUserIdAsync(string userId, int limit = 50)
    {
        return await _context.ChatMessages
            .Where(m => m.UserId == userId && m.CourseId == null)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage entity)
    {
        _context.ChatMessages.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<ChatMessage?> GetByIdAsync(int id)
    {
        return await _context.ChatMessages.FindAsync(id);
    }

    public async Task<List<ChatMessage>> GetAllAsync()
    {
        return await _context.ChatMessages.ToListAsync();
    }

    public async Task<ChatMessage> UpdateAsync(ChatMessage entity)
    {
        _context.ChatMessages.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.ChatMessages.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteByUserIdAsync(string userId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.UserId == userId && m.CourseId == null)
            .ToListAsync();

        if (messages.Any())
        {
            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ChatMessage>> GetByCourseIdAsync(string userId, int courseId, int limit = 50)
    {
        return await _context.ChatMessages
            .Where(m => m.UserId == userId && m.CourseId == courseId)
            .OrderByDescending(m => m.Timestamp)
            .Take(limit)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    public async Task DeleteByCourseIdAsync(string userId, int courseId)
    {
        var messages = await _context.ChatMessages
            .Where(m => m.UserId == userId && m.CourseId == courseId)
            .ToListAsync();

        if (messages.Any())
        {
            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }
}
