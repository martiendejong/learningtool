using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface IChatMessageRepository
{
    Task<List<ChatMessage>> GetByUserIdAsync(string userId, int limit = 50);
    Task<ChatMessage> CreateAsync(ChatMessage entity);
    Task<ChatMessage?> GetByIdAsync(int id);
    Task<List<ChatMessage>> GetAllAsync();
    Task<ChatMessage> UpdateAsync(ChatMessage entity);
    Task DeleteAsync(int id);
}
