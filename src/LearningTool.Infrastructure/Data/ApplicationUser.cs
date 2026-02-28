using Microsoft.AspNetCore.Identity;

namespace LearningTool.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
