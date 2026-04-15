namespace LearningTool.Domain.Entities;

/// <summary>A named collection of skills that can be licensed to organizations.</summary>
public class Bundle
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BundleSkill> BundleSkills { get; set; } = new List<BundleSkill>();
    public ICollection<OrganizationBundle> OrganizationBundles { get; set; } = new List<OrganizationBundle>();
    public ICollection<UserBundle> UserBundles { get; set; } = new List<UserBundle>();
}
