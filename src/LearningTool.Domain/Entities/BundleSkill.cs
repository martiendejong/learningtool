namespace LearningTool.Domain.Entities;

/// <summary>Maps a skill into a bundle.</summary>
public class BundleSkill
{
    public int Id { get; set; }
    public int BundleId { get; set; }
    public Bundle Bundle { get; set; } = null!;

    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}
