using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentTypesController : BaseController
{
    private readonly IIncidentTypeService _incidentTypeService;

    public IncidentTypesController(IIncidentTypeService incidentTypeService)
    {
        _incidentTypeService = incidentTypeService;
    }

    /// <summary>
    /// Get all incident types for all agencies
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<IncidentTypesByAgencyDto>>> GetAllIncidentTypes()
    {
        try
        {
            var incidentTypes = await _incidentTypeService.GetAllIncidentTypesAsync();
            return Ok(incidentTypes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving incident types", error = ex.Message });
        }
    }

    /// <summary>
    /// Get incident types for a specific agency
    /// </summary>
    [HttpGet("agency/{agencyName}")]
    [AllowAnonymous]
    public async Task<ActionResult<IncidentTypesByAgencyDto>> GetIncidentTypesByAgency(string agencyName)
    {
        try
        {
            var incidentTypes = await _incidentTypeService.GetIncidentTypesByAgencyAsync(agencyName);
            if (incidentTypes == null)
            {
                return NotFound($"Incident types not found for agency: {agencyName}");
            }

            return Ok(incidentTypes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Error retrieving incident types for agency: {agencyName}", error = ex.Message });
        }
    }
}