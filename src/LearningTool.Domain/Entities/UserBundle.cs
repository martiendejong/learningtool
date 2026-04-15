namespace LearningTool.Domain.Entities;

/// <summary>Tracks which users have been assigned which bundles.</summary>
public class UserBundle
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public int BundleId { get; set; }
    public Bundle Bundle { get; set; } = null!;

    public string? AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
