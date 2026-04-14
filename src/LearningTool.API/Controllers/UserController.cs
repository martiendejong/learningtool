using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET /api/user - List all users (admin only)
    [HttpGet]
    [Authorize(Roles = "SYSTEMADMIN")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users.ToList();

        var userDtos = new List<object>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                emailConfirmed = user.EmailConfirmed,
                role = roles.FirstOrDefault() ?? "INDIVIDUAL",
                organizationId = user.OrganizationId,
                createdAt = user.CreatedAt
            });
        }

        return Ok(userDtos);
    }

    // POST /api/user - Create user (admin only)
    [HttpPost]
    [Authorize(Roles = "SYSTEMADMIN")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email already exists" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true, // Admin-created users are pre-confirmed
            OrganizationId = request.OrganizationId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "User creation failed", errors = result.Errors });
        }

        // Assign role
        var role = request.Role ?? "STUDENT";
        if (await _roleManager.RoleExistsAsync(role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName,
            role,
            organizationId = user.OrganizationId
        });
    }

    // PUT /api/user/{id} - Update user (admin only)
    [HttpPut("{id}")]
    [Authorize(Roles = "SYSTEMADMIN")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        // Update email if provided
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
        {
            user.Email = request.Email;
            user.UserName = request.Email;
        }

        // Update organization if provided
        if (request.OrganizationId.HasValue)
        {
            user.OrganizationId = request.OrganizationId.Value == 0 ? null : request.OrganizationId;
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, request.Password);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(new { message = "User update failed", errors = updateResult.Errors });
        }

        // Update role if provided
        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (await _roleManager.RoleExistsAsync(request.Role))
            {
                await _userManager.AddToRoleAsync(user, request.Role);
            }
        }

        return Ok(new { message = "User updated successfully" });
    }

    // DELETE /api/user/{id} - Delete user (admin only)
    [HttpDelete("{id}")]
    [Authorize(Roles = "SYSTEMADMIN")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Prevent admin from deleting themselves
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == id)
        {
            return BadRequest(new { message = "Cannot delete your own account" });
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "User deletion failed", errors = result.Errors });
        }

        return Ok(new { message = "User deleted successfully" });
    }

    // GET /api/user/roles - Get available roles
    [HttpGet("roles")]
    [Authorize(Roles = "SYSTEMADMIN")]
    public IActionResult GetRoles()
    {
        var roles = new[]
        {
            new { value = "SYSTEMADMIN", label = "System Administrator", description = "Full system access" },
            new { value = "ORGADMIN", label = "Organization Administrator", description = "Manages a single organization's students and content" },
            new { value = "STUDENT", label = "Student", description = "Org-assigned learner with access to organization content" },
            new { value = "INDIVIDUAL", label = "Individual", description = "Self-registered learner" }
        };

        return Ok(roles);
    }

    // GET /api/user/current - Get current user
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            userName = user.UserName,
            role = roles.FirstOrDefault() ?? "INDIVIDUAL",
            organizationId = user.OrganizationId
        });
    }
}

public record CreateUserRequest(string Email, string Password, string? Role = "INDIVIDUAL", int? OrganizationId = null);
public record UpdateUserRequest(string? Email = null, string? Password = null, string? Role = null, int? OrganizationId = null);
