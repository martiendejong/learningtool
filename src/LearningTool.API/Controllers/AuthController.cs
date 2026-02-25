using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LearningTool.API.Models;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new { message = "Email already registered" });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            AccountType = request.AccountType ?? "Individual",
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Registration failed", errors = result.Errors });
        }

        var token = await GenerateJwtTokenAsync(user);
        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                accountType = user.AccountType,
                organizationId = user.OrganizationId,
                roleInOrganization = user.RoleInOrganization,
                profilePictureUrl = user.ProfilePictureUrl
            }
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email and password are required" });
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        // Update last login timestamp
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var token = await GenerateJwtTokenAsync(user);
        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                accountType = user.AccountType,
                organizationId = user.OrganizationId,
                roleInOrganization = user.RoleInOrganization,
                profilePictureUrl = user.ProfilePictureUrl
            }
        });
    }

    // GET /api/auth/google
    [HttpGet("google")]
    public IActionResult GoogleLogin([FromQuery] string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    // GET /api/auth/google/callback
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string? returnUrl = null)
    {
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error=google_login_failed");
        }

        // Try to sign in with existing Google account
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                var token = await GenerateJwtTokenAsync(user);
                var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/auth/callback?token={token}");
            }
        }

        // New Google user - create account
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        var googleId = info.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = info.Principal.FindFirstValue(ClaimTypes.Name);
        var picture = info.Principal.FindFirstValue("picture");

        if (string.IsNullOrEmpty(email))
        {
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error=no_email");
        }

        // Check if email already exists (linked to password account)
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            // Link Google account to existing email account
            var addLoginResult = await _userManager.AddLoginAsync(existingUser, info);
            if (addLoginResult.Succeeded)
            {
                existingUser.GoogleId = googleId;
                existingUser.ProfilePictureUrl ??= picture;
                existingUser.FullName ??= name;
                existingUser.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);

                var token = await GenerateJwtTokenAsync(existingUser);
                var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/auth/callback?token={token}");
            }

            var frontendUrlError = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrlError}/login?error=link_failed");
        }

        // Create new user from Google account
        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true, // Google verified
            GoogleId = googleId,
            FullName = name,
            ProfilePictureUrl = picture,
            AccountType = "Individual" // Default for OAuth
        };

        var createResult = await _userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
        {
            var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error=create_failed");
        }

        await _userManager.AddLoginAsync(newUser, info);
        await _userManager.AddToRoleAsync(newUser, "Student");

        var newToken = await GenerateJwtTokenAsync(newUser);
        var newFrontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:5173";
        return Redirect($"{newFrontendUrl}/auth/callback?token={newToken}");
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "LearningTool";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "LearningToolUsers";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("account_type", user.AccountType ?? "Individual")
        };

        // Add organization claims if user belongs to an organization
        if (user.OrganizationId.HasValue)
        {
            claims.Add(new Claim("organization_id", user.OrganizationId.Value.ToString()));
            if (!string.IsNullOrEmpty(user.RoleInOrganization))
            {
                claims.Add(new Claim("organization_role", user.RoleInOrganization));
            }
        }

        // Add user roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // POST /api/auth/accept-invite
    [HttpPost("accept-invite")]
    public async Task<IActionResult> AcceptInvite([FromBody] AcceptInviteRequest request)
    {
        // TODO: Validate invitation token via Hazina Dynamic API
        // For now, mock validation
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new { message = "Invalid invitation token" });
        }

        // Check if user exists
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true, // Email verified via invitation
                FullName = request.FullName,
                AccountType = "Organization",
                // TODO: Get OrganizationId and Role from invitation token
                // OrganizationId = invitation.OrganizationId,
                // RoleInOrganization = invitation.Role
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "User creation failed", errors = result.Errors });
            }

            await _userManager.AddToRoleAsync(user, "Student");
        }
        else
        {
            // Update existing user to join organization
            // TODO: Get OrganizationId and Role from invitation token
            // user.OrganizationId = invitation.OrganizationId;
            // user.RoleInOrganization = invitation.Role;
            user.AccountType = "Organization";

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "User update failed", errors = result.Errors });
            }
        }

        // TODO: Mark invitation as accepted via Hazina Dynamic API

        var jwtToken = await GenerateJwtTokenAsync(user);
        return Ok(new
        {
            token = jwtToken,
            user = new
            {
                id = user.Id,
                email = user.Email,
                userName = user.UserName,
                fullName = user.FullName,
                accountType = user.AccountType,
                organizationId = user.OrganizationId,
                roleInOrganization = user.RoleInOrganization,
                profilePictureUrl = user.ProfilePictureUrl
            }
        });
    }
}

public record RegisterRequest(string Email, string Password, string? FullName = null, string? AccountType = "Individual");
public record LoginRequest(string Email, string Password);
public record AcceptInviteRequest(string Token, string Email, string Password, string? FullName = null);
