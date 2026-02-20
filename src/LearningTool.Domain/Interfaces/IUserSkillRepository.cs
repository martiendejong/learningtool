using LearningTool.Domain.Entities;

namespace LearningTool.Domain.Interfaces;

public interface IUserSkillRepository
{
    Task<List<UserSkill>> GetByUserIdAsync(string userId, bool includeDeleted = false);
    Task<UserSkill?> GetByUserAndSkillAsync(string userId, int skillId);
    Task<UserSkill> CreateAsync(UserSkill userSkill);
    Task UpdateAsync(UserSkill userSkill);
    Task SoftDeleteAsync(string userId, int skillId);
}
