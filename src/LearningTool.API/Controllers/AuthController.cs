using LearningTool.API.Services;
using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly LearningToolDbContext _context;
    private readonly EmailService _email;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        LearningToolDbContext context,
        EmailService email)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
        _email = email;
    }

    // POST /auth/register
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required" });

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Email already registered" });

        // Resolve invite token if provided
        Invitation? invite = null;
        if (!string.IsNullOrWhiteSpace(request.InviteToken))
        {
            var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.InviteToken)));
            invite = await _context.Invitations
                .FirstOrDefaultAsync(i => i.TokenHash == tokenHash
                    && i.ExpiresAt > DateTime.UtcNow
                    && i.UsedCount < i.MaxUses);

            if (invite == null)
                return BadRequest(new { message = "Invite link is invalid or expired" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            OrganizationId = invite?.OrganizationId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = "Registration failed", errors = result.Errors });

        var role = invite != null ? "STUDENT" : "INDIVIDUAL";
        await _userManager.AddToRoleAsync(user, role);

        if (invite != null)
        {
            invite.UsedCount++;
            await _context.SaveChangesAsync();
        }

        var jwtToken = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            token = jwtToken,
            user = MapUser(user, roles.FirstOrDefault() ?? role)
        });
    }

    // POST /auth/register-organization — creates an org + OrgAdmin in one transaction
    [HttpPost("register-organization")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RegisterOrganization([FromBody] RegisterOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required" });
        if (string.IsNullOrWhiteSpace(request.OrganizationName))
            return BadRequest(new { message = "Organization name is required" });

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Email already registered" });

        var existingOrg = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Name == request.OrganizationName && !o.IsDeleted);
        if (existingOrg != null)
            return BadRequest(new { message = "An organization with that name already exists" });

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var org = new Organization
            {
                Name = request.OrganizationName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                OrganizationId = org.Id,
                EmailConfirmed = true
            };
            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                await tx.RollbackAsync();
                return BadRequest(new { message = "Registration failed", errors = createResult.Errors });
            }
            await _userManager.AddToRoleAsync(user, "ORGADMIN");
            await tx.CommitAsync();

            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                token,
                user = MapUser(user, roles.FirstOrDefault() ?? "ORGADMIN"),
                organization = new { org.Id, org.Name }
            });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // POST /auth/login
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required" });

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            token,
            user = MapUser(user, roles.FirstOrDefault() ?? "INDIVIDUAL")
        });
    }

    // POST /auth/logout
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        await _userManager.UpdateSecurityStampAsync(user);
        return Ok(new { message = "Logged out successfully" });
    }

    // ── Google OAuth ──────────────────────────────────────────────────────────

    // GET /auth/google  — initiates Google sign-in
    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? next)
    {
        if (string.IsNullOrEmpty(_configuration["Google:ClientId"]) ||
            string.IsNullOrEmpty(_configuration["Google:ClientSecret"]))
            return StatusCode(503, new { message = "Google sign-in is not configured on this server." });

        var redirectUrl = Url.Action(nameof(GoogleComplete), "Auth", new { next });
        var props = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    // GET /auth/google/complete  — called after Google redirects back
    [HttpGet("google/complete")]
    public async Task<IActionResult> GoogleComplete([FromQuery] string? next)
    {
        var result = await HttpContext.AuthenticateAsync("GoogleTemp");
        if (!result.Succeeded)
            return Unauthorized(new { message = "Google authentication failed" });

        var googleId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var googleEmail = result.Principal?.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(googleEmail))
            return Unauthorized(new { message = "Google did not return required account information" });

        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";

        // Existing user — link GoogleId if needed and issue JWT
        var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId || u.Email == googleEmail);

        if (existingUser != null)
        {
            if (existingUser.GoogleId == null)
            {
                existingUser.GoogleId = googleId;
                await _userManager.UpdateAsync(existingUser);
            }
            var token = await GenerateJwtToken(existingUser);
            var safePath = IsValidRelativePath(next) ? next! : "/dashboard";
            // Pass JWT in query param so the frontend can store it
            return Redirect($"{frontendUrl}{safePath}?token={Uri.EscapeDataString(token)}");
        }

        // New user — generate OTP and redirect to verify page
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var stale = await _context.PendingGoogleVerifications
            .Where(p => p.Email == googleEmail)
            .ToListAsync();
        _context.PendingGoogleVerifications.RemoveRange(stale);
        _context.PendingGoogleVerifications.Add(new PendingGoogleVerification
        {
            Email = googleEmail,
            GoogleId = googleId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        });
        await _context.SaveChangesAsync();

        await _email.SendOtpAsync(googleEmail, code);

        var encodedEmail = Uri.EscapeDataString(googleEmail);
        return Redirect($"{frontendUrl}/verify-email?email={encodedEmail}");
    }

    // POST /auth/verify-google  — submit the OTP received by email
    [HttpPost("verify-google")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyGoogle([FromBody] VerifyGoogleRequest request)
    {
        var pending = await _context.PendingGoogleVerifications
            .FirstOrDefaultAsync(p => p.Email == request.Email);

        if (pending == null)
            return BadRequest(new { message = "No pending verification found for this email" });

        if (pending.ExpiresAt < DateTime.UtcNow)
        {
            _context.PendingGoogleVerifications.Remove(pending);
            await _context.SaveChangesAsync();
            return BadRequest(new { message = "Verification code has expired. Please sign in with Google again." });
        }

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(pending.Code),
                Encoding.UTF8.GetBytes(request.Code)))
            return BadRequest(new { message = "Incorrect verification code" });

        // Create the user
        var user = new ApplicationUser
        {
            UserName = pending.Email,
            Email = pending.Email,
            GoogleId = pending.GoogleId,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            // Race condition: user was created between OTP send and verify — link GoogleId
            var existing = await _userManager.FindByEmailAsync(pending.Email);
            if (existing != null && existing.GoogleId == null)
            {
                existing.GoogleId = pending.GoogleId;
                await _userManager.UpdateAsync(existing);
                user = existing;
            }
            else if (existing != null)
            {
                user = existing;
            }
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "INDIVIDUAL");
        }

        _context.PendingGoogleVerifications.Remove(pending);
        await _context.SaveChangesAsync();

        var token = await GenerateJwtToken(user);
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            token,
            user = MapUser(user, roles.FirstOrDefault() ?? "INDIVIDUAL")
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LearningTool";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "LearningToolUsers";

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("SecurityStamp", user.SecurityStamp ?? "")
        };

        if (user.OrganizationId.HasValue)
            claims.Add(new Claim("organizationId", user.OrganizationId.Value.ToString()));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static object MapUser(ApplicationUser u, string role) => new
    {
        id = u.Id,
        email = u.Email,
        userName = u.UserName,
        role,
        organizationId = u.OrganizationId,
        hasGoogleLogin = u.GoogleId != null
    };

    private static bool IsValidRelativePath(string? path) =>
        !string.IsNullOrEmpty(path) &&
        Uri.TryCreate(path, UriKind.Relative, out _) &&
        path.StartsWith('/') &&
        !path.StartsWith("//") &&
        !path.StartsWith("/\\");
}

public record RegisterRequest(string Email, string Password, string? InviteToken = null);
public record RegisterOrganizationRequest(string Email, string Password, string OrganizationName);
public record LoginRequest(string Email, string Password);
public record VerifyGoogleRequest(string Email, string Code);
