using LearningTool.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace LearningTool.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? OrganizationId { get; set; }

    // Navigation property
    public Organization? Organization { get; set; }
}
