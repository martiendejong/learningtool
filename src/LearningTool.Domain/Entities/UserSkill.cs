namespace LearningTool.Domain.Entities;

public class UserSkill
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int SkillId { get; set; }
    public UserSkillStatus Status { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime AddedAt { get; set; }

    // Navigation properties
    public Skill Skill { get; set; } = null!;
}

public enum UserSkillStatus
{
    WantToLearn,
    InProgress,
    Mastered
}
