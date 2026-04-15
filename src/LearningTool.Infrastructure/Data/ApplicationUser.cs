using LearningTool.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LearningTool.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? OrganizationId { get; set; }

    /// <summary>Set when a user authenticates via Google OAuth. Null for password-only accounts.</summary>
    public string? GoogleId { get; set; }

    // Navigation property
    public Organization? Organization { get; set; }
}
