using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill?> GetByNameAsync(string name);
    Task<List<Skill>> GetAllAsync(bool includeDeleted = false);
    Task<List<Skill>> SearchAsync(string query, bool includeDeleted = false);
    Task<Skill> CreateAsync(Skill skill);
    Task UpdateAsync(Skill skill);
    Task SoftDeleteAsync(int id);
}
