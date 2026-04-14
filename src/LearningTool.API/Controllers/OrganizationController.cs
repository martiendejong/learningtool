using LearningTool.Domain.Entities;
using LearningTool.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningTool.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SYSTEMADMIN")]
public class OrganizationController : ControllerBase
{
    private readonly LearningToolDbContext _context;

    public OrganizationController(LearningToolDbContext context)
    {
        _context = context;
    }

    // GET /api/organization - List all organizations
    [HttpGet]
    public async Task<IActionResult> GetOrganizations()
    {
        var organizations = await _context.Organizations
            .Where(o => !o.IsDeleted)
            .OrderBy(o => o.Name)
            .ToListAsync();

        return Ok(organizations);
    }

    // GET /api/organization/{id} - Get organization by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrganization(int id)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (organization == null)
        {
            return NotFound(new { message = "Organization not found" });
        }

        return Ok(organization);
    }

    // POST /api/organization - Create organization
    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { message = "Organization name is required" });
        }

        // Check if organization name already exists
        var existingOrg = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Name == request.Name && !o.IsDeleted);

        if (existingOrg != null)
        {
            return BadRequest(new { message = "Organization with this name already exists" });
        }

        var organization = new Organization
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        return Ok(organization);
    }

    // PUT /api/organization/{id} - Update organization
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateOrganizationRequest request)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (organization == null)
        {
            return NotFound(new { message = "Organization not found" });
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            // Check if new name conflicts with existing organization
            var existingOrg = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Name == request.Name && o.Id != id && !o.IsDeleted);

            if (existingOrg != null)
            {
                return BadRequest(new { message = "Organization with this name already exists" });
            }

            organization.Name = request.Name;
        }

        if (request.Description != null)
        {
            organization.Description = request.Description;
        }

        if (request.IsActive.HasValue)
        {
            organization.IsActive = request.IsActive.Value;
        }

        await _context.SaveChangesAsync();

        return Ok(organization);
    }

    // DELETE /api/organization/{id} - Delete organization (soft delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrganization(int id)
    {
        var organization = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

        if (organization == null)
        {
            return NotFound(new { message = "Organization not found" });
        }

        // Check if organization has any active users
        var userCount = await _context.Users
            .CountAsync(u => u.OrganizationId == id);

        if (userCount > 0)
        {
            return BadRequest(new { message = $"Cannot delete organization with {userCount} active user(s). Please reassign users first." });
        }

        // Soft delete
        organization.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Organization deleted successfully" });
    }
}

public record CreateOrganizationRequest(string Name, string? Description = null);
public record UpdateOrganizationRequest(string? Name = null, string? Description = null, bool? IsActive = null);
