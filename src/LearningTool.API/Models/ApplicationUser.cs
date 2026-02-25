using Microsoft.AspNetCore.Identity;

namespace LearningTool.API.Models;

/// <summary>
/// Extended user model with organization and OAuth support
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Account type: "Individual" or "Organization"
    /// </summary>
    public string AccountType { get; set; } = "Individual";

    /// <summary>
    /// Organization ID if user belongs to an organization (null for individual accounts)
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Role within organization: "Admin" or "Student" (null for individual accounts)
    /// </summary>
    public string? RoleInOrganization { get; set; }

    /// <summary>
    /// Google OAuth subject ID (unique Google user identifier)
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// Profile picture URL (from Google OAuth or uploaded)
    /// </summary>
    public string? ProfilePictureUrl { get; set; }

    /// <summary>
    /// User's full name
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// Account creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login timestamp (updated on each successful login)
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
