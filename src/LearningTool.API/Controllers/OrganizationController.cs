using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using LearningTool.API.Data;
using LearningTool.API.Models;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganizationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<OrganizationController> _logger;

    public OrganizationController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<OrganizationController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET /api/organization/my - Get current user's organization
    [HttpGet("my")]
    public async Task<IActionResult> GetMyOrganization()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.OrganizationId.HasValue)
        {
            return NotFound(new { message = "User is not part of an organization" });
        }

        // TODO: Query Organization entity from Hazina Dynamic API
        // For now, return basic organization info from user
        return Ok(new
        {
            id = user.OrganizationId,
            userRole = user.RoleInOrganization,
            accountType = user.AccountType
        });
    }

    // GET /api/organization/{id}/members - Get organization members
    [HttpGet("{id}/members")]
    public async Task<IActionResult> GetMembers(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null || currentUser.OrganizationId != id)
        {
            return Forbid(); // User doesn't belong to this organization
        }

        if (currentUser.RoleInOrganization != "Admin")
        {
            return Forbid(); // Only admins can view members
        }

        var members = await _context.Users
            .Where(u => u.OrganizationId == id)
            .Select(u => new
            {
                id = u.Id,
                email = u.Email,
                fullName = u.FullName,
                roleInOrganization = u.RoleInOrganization,
                profilePictureUrl = u.ProfilePictureUrl,
                createdAt = u.CreatedAt,
                lastLoginAt = u.LastLoginAt
            })
            .ToListAsync();

        return Ok(members);
    }

    // POST /api/organization/invite - Send invitation
    [HttpPost("invite")]
    public async Task<IActionResult> InviteUser([FromBody] InviteRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null || !currentUser.OrganizationId.HasValue)
        {
            return BadRequest(new { message = "User is not part of an organization" });
        }

        if (currentUser.RoleInOrganization != "Admin")
        {
            return Forbid(); // Only admins can invite
        }

        // Check if email already exists in organization
        var existingMember = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.OrganizationId == currentUser.OrganizationId);

        if (existingMember != null)
        {
            return BadRequest(new { message = "User is already a member of this organization" });
        }

        // Generate secure token (32 bytes, base64)
        var tokenBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        var token = Convert.ToBase64String(tokenBytes).Replace("+", "-").Replace("/", "_").Replace("=", "");

        // TODO: Create OrganizationInvitation via Hazina Dynamic API
        // For now, log the invitation details
        _logger.LogInformation(
            "Invitation created: Email={Email}, Token={Token}, OrgId={OrgId}, Role={Role}, InvitedBy={InvitedBy}",
            request.Email, token, currentUser.OrganizationId, request.Role ?? "Student", userId);

        // TODO: Send email via SendGrid
        var inviteUrl = $"{Request.Scheme}://{Request.Host}/invite/{token}";
        _logger.LogInformation("Invite URL: {InviteUrl}", inviteUrl);

        return Ok(new
        {
            message = "Invitation sent successfully",
            token,
            inviteUrl,
            expiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    // DELETE /api/organization/invite/{token} - Revoke invitation
    [HttpDelete("invite/{token}")]
    public async Task<IActionResult> RevokeInvitation(string token)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null || !currentUser.OrganizationId.HasValue)
        {
            return BadRequest(new { message = "User is not part of an organization" });
        }

        if (currentUser.RoleInOrganization != "Admin")
        {
            return Forbid();
        }

        // TODO: Update OrganizationInvitation status via Hazina Dynamic API
        _logger.LogInformation("Revoking invitation: Token={Token}", token);

        return Ok(new { message = "Invitation revoked successfully" });
    }

    // DELETE /api/organization/members/{memberId} - Remove member
    [HttpDelete("members/{memberId}")]
    public async Task<IActionResult> RemoveMember(string memberId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var currentUser = await _userManager.FindByIdAsync(userId);
        if (currentUser == null || !currentUser.OrganizationId.HasValue)
        {
            return BadRequest(new { message = "User is not part of an organization" });
        }

        if (currentUser.RoleInOrganization != "Admin")
        {
            return Forbid();
        }

        var member = await _userManager.FindByIdAsync(memberId);
        if (member == null || member.OrganizationId != currentUser.OrganizationId)
        {
            return NotFound(new { message = "Member not found in organization" });
        }

        // Cannot remove yourself
        if (member.Id == userId)
        {
            return BadRequest(new { message = "Cannot remove yourself from organization" });
        }

        // Remove organization association
        member.OrganizationId = null;
        member.RoleInOrganization = null;
        member.AccountType = "Individual";

        var result = await _userManager.UpdateAsync(member);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Failed to remove member", errors = result.Errors });
        }

        return Ok(new { message = "Member removed successfully" });
    }
}

public record InviteRequest(string Email, string? Role = "Student");
