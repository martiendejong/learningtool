using LearningTool.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LearningTool.Infrastructure.Data;

public class LearningToolDbContext : IdentityDbContext<ApplicationUser>
{
    public LearningToolDbContext(DbContextOptions<LearningToolDbContext> options)
        : base(options)
    {
    }

    // Catalog tables (shared across users)
    public DbSet<Skill> Skills { get; set; } = null!;
    public DbSet<Topic> Topics { get; set; } = null!;
    public DbSet<Course> Courses { get; set; } = null!;

    // User association tables
    public DbSet<UserSkill> UserSkills { get; set; } = null!;
    public DbSet<UserTopic> UserTopics { get; set; } = null!;
    public DbSet<UserCourse> UserCourses { get; set; } = null!;

    // Chat history
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete global query filters
        modelBuilder.Entity<Skill>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Topic>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Course>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<UserSkill>().HasQueryFilter(us => !us.IsDeleted);
        modelBuilder.Entity<UserTopic>().HasQueryFilter(ut => !ut.IsDeleted);
        modelBuilder.Entity<UserCourse>().HasQueryFilter(uc => !uc.IsDeleted);

        // Skill configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Cascade delete: Skill → Topics
            entity.HasMany(e => e.Topics)
                .WithOne(e => e.Skill)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Topic configuration
        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            // Cascade delete: Topic → Courses
            entity.HasMany(e => e.Courses)
                .WithOne(e => e.Topic)
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Course configuration
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Content).HasColumnType("TEXT");
            entity.Property(e => e.CreatedAt).IsRequired();

            // Store Prerequisites as JSON
            entity.Property(e => e.Prerequisites)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
                )
                .HasColumnType("TEXT");

            // Store ResourceLinks as JSON
            entity.Property(e => e.ResourceLinks)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<ResourceLink>>(v, (JsonSerializerOptions?)null) ?? new List<ResourceLink>()
                )
                .HasColumnType("TEXT");
        });

        // UserSkill configuration
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.SkillId }).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.AddedAt).IsRequired();

            entity.HasOne(e => e.Skill)
                .WithMany()
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Restrict);  // Don't cascade - soft delete only
        });

        // UserTopic configuration
        modelBuilder.Entity<UserTopic>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.TopicId }).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.AddedAt).IsRequired();

            entity.HasOne(e => e.Topic)
                .WithMany()
                .HasForeignKey(e => e.TopicId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // UserCourse configuration
        modelBuilder.Entity<UserCourse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ChatMessage configuration
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.CourseId });  // Index for course-specific chat queries
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.Timestamp).IsRequired();

            // Store ToolCalls as JSON
            // ToolCalls is stored as JSON string
            entity.Property(e => e.ToolCalls).HasColumnType("TEXT");

            // Course relationship (optional - null for general chat)
            entity.HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
