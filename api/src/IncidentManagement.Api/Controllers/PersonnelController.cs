using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Api.Services;

namespace IncidentManagement.Api.Controllers;

/// <summary>
/// Controller for managing personnel with agency-based filtering
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonnelController : BaseController
{
    private readonly IPersonnelService _personnelService;
    private readonly IncidentManagementDbContext _context;
    private readonly IRealTimeNotificationService _realTimeService;

    public PersonnelController(IPersonnelService personnelService, IncidentManagementDbContext context, IRealTimeNotificationService realTimeService)
    {
        _personnelService = personnelService;
        _context = context;
        _realTimeService = realTimeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PersonnelDto>>> GetPersonnel(
        [FromQuery] int? stationId = null,
        [FromQuery] bool? isActive = null)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (userAgencyId == null)
        {
            return Unauthorized("User agency information not found");
        }

        var personnel = await _personnelService.GetCrewByStationAsync(stationId, isActive);
        
        // Filter personnel by user's agency
        var filteredPersonnel = personnel.Where(p => p.AgencyId == userAgencyId).ToList();
        
        return Ok(filteredPersonnel);
    }

    [HttpGet("grouped-by-station")]
    public async Task<ActionResult<Dictionary<string, List<PersonnelDto>>>> GetPersonnelGroupedByStation(
        [FromQuery] int? stationId = null,
        [FromQuery] bool? isActive = null)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (userAgencyId == null)
        {
            return Unauthorized("User agency information not found");
        }

        var groupedPersonnel = await _personnelService.GetCrewGroupedByStationAsync(isActive);
        
        // Filter grouped personnel by user's agency
        var filteredGroupedPersonnel = new Dictionary<string, List<PersonnelDto>>();
        foreach (var group in groupedPersonnel)
        {
            var filteredGroup = group.Value.Where(p => p.AgencyId == userAgencyId).ToList();
            if (filteredGroup.Any())
            {
                filteredGroupedPersonnel[group.Key.ToString()] = filteredGroup;
            }
        }
        
        return Ok(filteredGroupedPersonnel);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PersonnelDto>> GetPersonnel(int id)
    {
        var personnel = await _personnelService.GetPersonnelByIdAsync(id);
        if (personnel == null)
            return NotFound();

        return Ok(personnel);
    }

    [HttpPost]
    public async Task<ActionResult<PersonnelDto>> CreatePersonnel(CreatePersonnelDto dto)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (userAgencyId == null)
        {
            return Unauthorized("User agency information not found");
        }

        // Set the agency ID to the user's agency
        dto.AgencyId = userAgencyId.Value;

        // Validate that the station belongs to the user's agency
        var station = await _context.Stations
            .FirstOrDefaultAsync(s => s.Id == dto.StationId && s.AgencyId == userAgencyId);
        
        if (station == null)
        {
            return BadRequest("Station not found or does not belong to your agency");
        }

        var personnel = await _personnelService.CreatePersonnelAsync(dto);

        // Broadcast personnel creation
        await _realTimeService.BroadcastPersonnelCreated(personnel, personnel.StationId, personnel.AgencyId);

        return CreatedAtAction(nameof(GetPersonnel), new { id = personnel.Id }, personnel);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<PersonnelDto>> UpdatePersonnel(int id, UpdatePersonnelDto dto)
    {
        var personnel = await _personnelService.UpdatePersonnelAsync(id, dto);
        if (personnel == null)
            return NotFound();

        // Broadcast personnel update
        await _realTimeService.BroadcastPersonnelUpdated(personnel.Id, personnel, personnel.StationId, personnel.AgencyId);

        return Ok(personnel);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePersonnel(int id)
    {
        // Get personnel info before deletion for broadcasting
        var existingPersonnel = await _context.Personnel
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.Id == id);

        var success = await _personnelService.DeletePersonnelAsync(id);
        if (!success)
            return NotFound();

        // Broadcast personnel deletion
        if (existingPersonnel != null)
        {
            await _realTimeService.BroadcastPersonnelDeleted(id, existingPersonnel.StationId, existingPersonnel.Station.AgencyId);
        }

        return NoContent();
    }
}