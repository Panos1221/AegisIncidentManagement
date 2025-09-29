using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgenciesController : BaseController
{
    private readonly IncidentManagementDbContext _context;

    public AgenciesController(IncidentManagementDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all agencies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AgencyDto>>> GetAgencies()
    {
        try
        {
            var agencies = await _context.Agencies
                .Where(a => a.IsActive)
                .Select(a => new AgencyDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    Name = a.Name,
                    Code = a.Code,
                    Description = a.Description,
                    IsActive = a.IsActive
                })
                .ToListAsync();

            return Ok(agencies);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving agencies", error = ex.Message });
        }
    }

    /// <summary>
    /// Get agency by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AgencyDto>> GetAgency(int id)
    {
        try
        {
            var agency = await _context.Agencies
                .Where(a => a.Id == id && a.IsActive)
                .Select(a => new AgencyDto
                {
                    Id = a.Id,
                    Type = a.Type,
                    Name = a.Name,
                    Code = a.Code,
                    Description = a.Description,
                    IsActive = a.IsActive
                })
                .FirstOrDefaultAsync();

            if (agency == null)
                return NotFound();

            return Ok(agency);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving agency", error = ex.Message });
        }
    }
}