namespace LearningTool.Domain.Entities;

public class Invitation
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    /// <summary>SHA-256 hex hash of the raw token sent to the user.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }
    public int MaxUses { get; set; } = 10;
    public int UsedCount { get; set; }

    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
