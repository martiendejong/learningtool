using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LearningTool.API.Models;

namespace LearningTool.API.Data;

/// <summary>
/// Identity database context for authentication and user management
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Index for organization queries
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.OrganizationId)
            .HasDatabaseName("IX_Users_OrganizationId");

        // Index for Google OAuth lookups
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.GoogleId)
            .IsUnique()
            .HasFilter("[GoogleId] IS NOT NULL")
            .HasDatabaseName("IX_Users_GoogleId");

        // Index for account type filtering
        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.AccountType)
            .HasDatabaseName("IX_Users_AccountType");

        // Configure string lengths
        builder.Entity<ApplicationUser>()
            .Property(u => u.AccountType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .Property(u => u.RoleInOrganization)
            .HasMaxLength(20);

        builder.Entity<ApplicationUser>()
            .Property(u => u.FullName)
            .HasMaxLength(200);

        builder.Entity<ApplicationUser>()
            .Property(u => u.ProfilePictureUrl)
            .HasMaxLength(500);

        builder.Entity<ApplicationUser>()
            .Property(u => u.GoogleId)
            .HasMaxLength(100);
    }
}
