using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
#pragma warning disable CA1304

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/org")]
[Authorize(Roles = "ORGADMIN")]
public class OrgController : ControllerBase
{
    private readonly LearningToolDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrgController(LearningToolDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    /// <summary>OrgId from JWT — every query must filter by this to enforce tenant isolation.</summary>
    private int OrgId => int.Parse(User.FindFirst("organizationId")!.Value);
    private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    // ── Overview ──────────────────────────────────────────────────────────────

    // GET /api/org/overview
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var orgId = OrgId;

        var org = await _context.Organizations
            .Where(o => o.Id == orgId)
            .Select(o => new { o.Name })
            .FirstOrDefaultAsync();

        if (org is null) return NotFound();

        var students = await _userManager.Users
            .Where(u => u.OrganizationId == orgId)
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .ToListAsync();

        var studentIds = students.Select(s => s.Id).ToList();
        var totalCourses = await _context.Courses.CountAsync();

        // Aggregate completion per student
        var completionByStudent = await _context.UserCourses
            .Where(uc => studentIds.Contains(uc.UserId) && uc.Status == UserCourseStatus.Completed)
            .GroupBy(uc => uc.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count(),
                LastActivity = g.Max(uc => uc.CompletedAt)
            })
            .ToListAsync();

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var activeStudents = completionByStudent.Count(p => p.LastActivity >= thirtyDaysAgo);

        double avgCompletion = 0;
        if (totalCourses > 0 && students.Count > 0)
        {
            var totalCompleted = completionByStudent.Sum(p => p.Count);
            avgCompletion = Math.Round((double)totalCompleted / (students.Count * totalCourses) * 100, 1);
        }

        var recentStudents = students
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(s => new { s.Id, s.Email, joinedAt = s.CreatedAt });

        return Ok(new
        {
            orgName = org.Name,
            totalStudents = students.Count,
            activeStudents,
            totalCourses,
            avgCompletionPct = avgCompletion,
            recentStudents
        });
    }

    // ── Students ──────────────────────────────────────────────────────────────

    // GET /api/org/students?search=xxx
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] string? search)
    {
        var orgId = OrgId;

        var query = _userManager.Users.Where(u => u.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email!.Contains(search.Trim().ToLower()));

        var students = await query
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .ToListAsync();

        var studentIds = students.Select(s => s.Id).ToList();
        var totalCourses = await _context.Courses.CountAsync();

        var completionByStudent = await _context.UserCourses
            .Where(uc => studentIds.Contains(uc.UserId) && uc.Status == UserCourseStatus.Completed)
            .GroupBy(uc => uc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        var completionMap = completionByStudent.ToDictionary(p => p.UserId, p => p.Count);

        var result = students.Select(s =>
        {
            var completed = completionMap.GetValueOrDefault(s.Id, 0);
            return new
            {
                s.Id,
                s.Email,
                joinedAt = s.CreatedAt,
                coursesCompleted = completed,
                completionPct = totalCourses > 0
                    ? Math.Round((double)completed / totalCourses * 100, 1)
                    : 0.0
            };
        }).OrderByDescending(s => s.joinedAt);

        return Ok(result);
    }

    // GET /api/org/students/{userId}
    [HttpGet("students/{userId}")]
    public async Task<IActionResult> GetStudent(string userId)
    {
        var orgId = OrgId;

        var student = await _userManager.Users
            .Where(u => u.Id == userId && u.OrganizationId == orgId)
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .FirstOrDefaultAsync();

        if (student is null) return NotFound();

        // Per-skill breakdown — avoid nav property on Course by joining via UserCourses separately
        var completedCourseIds = await _context.UserCourses
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.Completed)
            .Select(uc => uc.CourseId)
            .ToListAsync();

        var skills = await _context.Skills
            .Select(s => new
            {
                s.Id,
                s.Name,
                CoursesInSkill = s.Topics.SelectMany(t => t.Courses).Select(c => c.Id).ToList()
            })
            .OrderBy(s => s.Name)
            .ToListAsync();

        var skillBreakdown = skills.Select(s => new
        {
            s.Id,
            s.Name,
            TotalCourses = s.CoursesInSkill.Count,
            CompletedCourses = s.CoursesInSkill.Count(id => completedCourseIds.Contains(id))
        }).ToList();

        var lastActivity = await _context.UserCourses
            .Where(uc => uc.UserId == userId && uc.Status == UserCourseStatus.Completed)
            .MaxAsync(uc => (DateTime?)uc.CompletedAt);

        var totalCompleted = skillBreakdown.Sum(s => s.CompletedCourses);
        var totalCourses = skillBreakdown.Sum(s => s.TotalCourses);

        return Ok(new
        {
            student.Id,
            student.Email,
            joinedAt = student.CreatedAt,
            totalCoursesCompleted = totalCompleted,
            lastActivity,
            completionPct = totalCourses > 0
                ? Math.Round((double)totalCompleted / totalCourses * 100, 1)
                : 0.0,
            skills = skillBreakdown.Select(s => new
            {
                s.Id,
                s.Name,
                s.TotalCourses,
                s.CompletedCourses,
                completionPct = s.TotalCourses > 0
                    ? Math.Round((double)s.CompletedCourses / s.TotalCourses * 100, 1)
                    : 0.0
            })
        });
    }

    // DELETE /api/org/students/{userId}  — remove from org, revert to INDIVIDUAL
    [HttpDelete("students/{userId}")]
    public async Task<IActionResult> RemoveStudent(string userId)
    {
        var orgId = OrgId;

        var student = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.OrganizationId == orgId);

        if (student is null) return NotFound();

        student.OrganizationId = null;
        await _userManager.UpdateAsync(student);

        var roles = await _userManager.GetRolesAsync(student);
        await _userManager.RemoveFromRolesAsync(student, roles);
        await _userManager.AddToRoleAsync(student, "INDIVIDUAL");

        return NoContent();
    }

    // ── Invites ───────────────────────────────────────────────────────────────

    // POST /api/org/invite
    [HttpPost("invite")]
    public async Task<IActionResult> CreateInvite([FromBody] CreateInviteRequest request)
    {
        var expiresInDays = request.ExpiresInDays ?? 7;
        var maxUses = request.MaxUses ?? 10;

        if (expiresInDays < 1 || expiresInDays > 365)
            return BadRequest(new { message = "Expiry must be between 1 and 365 days" });

        if (maxUses < 1 || maxUses > 500)
            return BadRequest(new { message = "Max uses must be between 1 and 500" });

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var invite = new Invitation
        {
            OrganizationId = OrgId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(expiresInDays),
            MaxUses = maxUses,
            CreatedByUserId = UserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Invitations.Add(invite);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            token = rawToken,
            inviteUrl = $"/register?invite={Uri.EscapeDataString(rawToken)}",
            expiresAt = invite.ExpiresAt,
            maxUses = invite.MaxUses
        });
    }

    // GET /api/org/invites
    [HttpGet("invites")]
    public async Task<IActionResult> GetInvites()
    {
        var orgId = OrgId;

        var invites = await _context.Invitations
            .Where(i => i.OrganizationId == orgId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new
            {
                i.Id,
                i.ExpiresAt,
                i.MaxUses,
                i.UsedCount,
                i.CreatedAt,
                isExpired = i.ExpiresAt < DateTime.UtcNow,
                isExhausted = i.UsedCount >= i.MaxUses
            })
            .ToListAsync();

        return Ok(invites);
    }

    // DELETE /api/org/invites/{inviteId}
    [HttpDelete("invites/{inviteId:int}")]
    public async Task<IActionResult> RevokeInvite(int inviteId)
    {
        var orgId = OrgId;

        var invite = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == inviteId && i.OrganizationId == orgId);

        if (invite is null) return NotFound();

        _context.Invitations.Remove(invite);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // ── Members ───────────────────────────────────────────────────────────────

    // ── Bundles ───────────────────────────────────────────────────────────────

    // GET /api/org/bundles
    [HttpGet("bundles")]
    public async Task<IActionResult> GetBundles()
    {
        var orgId = OrgId;

        var bundles = await _context.OrganizationBundles
            .Where(ob => ob.OrganizationId == orgId)
            .Select(ob => new
            {
                ob.Id,
                ob.BundleId,
                bundleName = ob.Bundle!.Name,
                bundleDescription = ob.Bundle.Description,
                ob.MaxUsers,
                ob.IsUnlimited,
                ob.CreatedAt,
                skillCount = ob.Bundle.BundleSkills.Count,
                skills = ob.Bundle.BundleSkills.Select(bs => new { bs.Skill.Id, bs.Skill.Name }),
                assignedUsers = _context.UserBundles
                    .Count(ub => ub.BundleId == ob.BundleId
                        && _userManager.Users.Any(u => u.Id == ub.UserId && u.OrganizationId == orgId))
            })
            .ToListAsync();

        return Ok(bundles);
    }

    // GET /api/org/bundles/{bundleId}/users
    [HttpGet("bundles/{bundleId:int}/users")]
    public async Task<IActionResult> GetBundleUsers(int bundleId)
    {
        var orgId = OrgId;

        if (!await _context.OrganizationBundles
                .AnyAsync(ob => ob.OrganizationId == orgId && ob.BundleId == bundleId))
            return NotFound(new { message = "Bundle not assigned to your organization" });

        var assignedUserIds = await _context.UserBundles
            .Where(ub => ub.BundleId == bundleId)
            .Select(ub => ub.UserId)
            .ToListAsync();

        var members = await _userManager.Users
            .Where(u => u.OrganizationId == orgId)
            .Select(u => new
            {
                u.Id,
                u.Email,
                isAssigned = assignedUserIds.Contains(u.Id)
            })
            .OrderBy(u => u.Email)
            .ToListAsync();

        return Ok(members);
    }

    // POST /api/org/bundles/{bundleId}/users/{userId}
    [HttpPost("bundles/{bundleId:int}/users/{userId}")]
    public async Task<IActionResult> AssignBundleToUser(int bundleId, string userId)
    {
        var orgId = OrgId;

        var orgBundle = await _context.OrganizationBundles
            .FirstOrDefaultAsync(ob => ob.OrganizationId == orgId && ob.BundleId == bundleId);
        if (orgBundle == null)
            return NotFound(new { message = "Bundle not assigned to your organization" });

        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.OrganizationId == orgId);
        if (user == null)
            return NotFound(new { message = "User not found in your organization" });

        if (await _context.UserBundles.AnyAsync(ub => ub.UserId == userId && ub.BundleId == bundleId))
            return BadRequest(new { message = "User already has this bundle" });

        // Seat limit check
        if (!orgBundle.IsUnlimited)
        {
            var currentCount = await _context.UserBundles
                .CountAsync(ub => ub.BundleId == bundleId
                    && _userManager.Users.Any(u => u.Id == ub.UserId && u.OrganizationId == orgId));

            if (currentCount >= orgBundle.MaxUsers)
                return BadRequest(new { message = $"Seat limit reached ({orgBundle.MaxUsers} max)" });
        }

        _context.UserBundles.Add(new UserBundle
        {
            UserId = userId,
            BundleId = bundleId,
            AssignedByUserId = UserId,
            AssignedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/org/bundles/{bundleId}/users/{userId}
    [HttpDelete("bundles/{bundleId:int}/users/{userId}")]
    public async Task<IActionResult> RemoveBundleFromUser(int bundleId, string userId)
    {
        var orgId = OrgId;

        if (!await _context.OrganizationBundles
                .AnyAsync(ob => ob.OrganizationId == orgId && ob.BundleId == bundleId))
            return NotFound(new { message = "Bundle not assigned to your organization" });

        var ub = await _context.UserBundles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.BundleId == bundleId);
        if (ub == null) return NotFound();

        _context.UserBundles.Remove(ub);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ── Members ───────────────────────────────────────────────────────────────

    // GET /api/org/members
    [HttpGet("members")]
    public async Task<IActionResult> GetMembers()
    {
        var orgId = OrgId;

        var members = await _userManager.Users
            .Where(u => u.OrganizationId == orgId)
            .Select(u => new { u.Id, u.Email, u.CreatedAt })
            .ToListAsync();

        var result = new List<object>();
        foreach (var m in members)
        {
            var user = await _userManager.FindByIdAsync(m.Id);
            var roles = user != null ? await _userManager.GetRolesAsync(user) : [];
            result.Add(new
            {
                m.Id,
                m.Email,
                role = roles.FirstOrDefault() ?? "STUDENT",
                joinedAt = m.CreatedAt
            });
        }

        return Ok(result);
    }
}

public record CreateInviteRequest(int? ExpiresInDays = 7, int? MaxUses = 10);
