using Microsoft.AspNetCore.Identity;

namespace LearningTool.API;

public static class RoleSeeder
{
    public static async Task SeedRolesAndAdmin(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        // Create roles if they don't exist
        string[] roles = { "ADMIN", "STUDENT" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create default admin user if configured
        var adminEmail = configuration["AdminUser:Email"] ?? "admin@learningtool.com";
        var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "ADMIN");
                Console.WriteLine($"Default admin user created: {adminEmail}");
            }
            else
            {
                Console.WriteLine($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Ensure existing user has admin role
            var roles_user = await userManager.GetRolesAsync(adminUser);
            if (!roles_user.Contains("ADMIN"))
            {
                await userManager.AddToRoleAsync(adminUser, "ADMIN");
            }
        }
    }
}
