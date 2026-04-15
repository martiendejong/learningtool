namespace LearningTool.Domain.Entities;

/// <summary>Licenses a bundle to an organization, optionally with a seat limit.</summary>
public class OrganizationBundle
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public int BundleId { get; set; }
    public Bundle Bundle { get; set; } = null!;

    public int MaxUsers { get; set; }
    public bool IsUnlimited { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
