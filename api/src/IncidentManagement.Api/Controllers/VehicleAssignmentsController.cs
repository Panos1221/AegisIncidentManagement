using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using IncidentManagement.Api.Services;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VehicleAssignmentsController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly IRealTimeNotificationService _realTimeService;

    public VehicleAssignmentsController(IncidentManagementDbContext context, IRealTimeNotificationService realTimeService)
    {
        _context = context;
        _realTimeService = realTimeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleAssignmentDto>>> GetVehicleAssignments([FromQuery] int? vehicleId = null, [FromQuery] int? stationId = null, [FromQuery] bool activeOnly = true)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (!userAgencyId.HasValue)
        {
            return Unauthorized("User agency information not found");
        }

        var query = _context.VehicleAssignments
            .Include(va => va.Vehicle)
                .ThenInclude(v => v.Station)
            .Include(va => va.Personnel)
                .ThenInclude(p => p.Station)
            .AsQueryable();

        // Apply agency-based filtering - only show assignments for vehicles/personnel in user's agency
        query = query.Where(va => va.Vehicle!.Station!.AgencyId == userAgencyId.Value);

        if (vehicleId.HasValue)
        {
            // Validate user can access this vehicle
            var vehicleExists = await _context.Vehicles
                .Include(v => v.Station)
                .AnyAsync(v => v.Id == vehicleId.Value && v.Station!.AgencyId == userAgencyId.Value);
            
            if (!vehicleExists)
            {
                return Forbid("You do not have permission to access this vehicle");
            }
            
            query = query.Where(va => va.VehicleId == vehicleId.Value);
        }

        if (stationId.HasValue)
        {
            // Validate user can access this station
            var stationExists = await _context.Stations
                .AnyAsync(s => s.Id == stationId.Value && s.AgencyId == userAgencyId.Value);
            
            if (!stationExists)
            {
                return Forbid("You do not have permission to access this station");
            }
            
            query = query.Where(va => va.Vehicle!.StationId == stationId.Value);
        }

        if (activeOnly)
            query = query.Where(va => va.IsActive);

        var assignments = await query.ToListAsync();

        var assignmentDtos = assignments.Select(va => new VehicleAssignmentDto
        {
            Id = va.Id,
            VehicleId = va.VehicleId,
            VehicleCallsign = va.Vehicle?.Callsign ?? "",
            PersonnelId = va.PersonnelId,
            PersonnelName = va.Personnel?.Name ?? "",
            PersonnelRank = va.Personnel?.Rank ?? "",
            AssignedAt = va.AssignedAt,
            UnassignedAt = va.UnassignedAt,
            IsActive = va.IsActive
        });

        return Ok(assignmentDtos);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleAssignmentDto>> CreateVehicleAssignment(CreateVehicleAssignmentDto createDto)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (!userAgencyId.HasValue)
        {
            return Unauthorized("User agency information not found");
        }

        // Validate that both vehicle and personnel belong to user's agency
        var vehicle = await _context.Vehicles
            .Include(v => v.Station)
            .FirstOrDefaultAsync(v => v.Id == createDto.VehicleId);
        
        var personnel = await _context.Personnel
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.Id == createDto.PersonnelId);

        if (vehicle == null || vehicle.Station?.AgencyId != userAgencyId.Value)
        {
            return Forbid("You do not have permission to assign this vehicle");
        }

        if (personnel == null || personnel.Station?.AgencyId != userAgencyId.Value)
        {
            return Forbid("You do not have permission to assign this personnel");
        }

        // Check if personnel is already assigned to another vehicle
        var existingAssignment = await _context.VehicleAssignments
            .FirstOrDefaultAsync(va => va.PersonnelId == createDto.PersonnelId && va.IsActive);

        if (existingAssignment != null)
        {
            return BadRequest("Personnel is already assigned to another vehicle");
        }

        var assignment = new VehicleAssignment
        {
            VehicleId = createDto.VehicleId,
            PersonnelId = createDto.PersonnelId
        };

        _context.VehicleAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        // Load related data
        await _context.Entry(assignment)
            .Reference(va => va.Vehicle)
            .LoadAsync();
        await _context.Entry(assignment)
            .Reference(va => va.Personnel)
            .LoadAsync();

        // Send real-time notification
        await _realTimeService.BroadcastVehicleAssignmentChanged(
            assignment.VehicleId,
            assignment.PersonnelId,
            "assigned",
            vehicle.StationId
        );

        var assignmentDto = new VehicleAssignmentDto
        {
            Id = assignment.Id,
            VehicleId = assignment.VehicleId,
            VehicleCallsign = assignment.Vehicle?.Callsign ?? "",
            PersonnelId = assignment.PersonnelId,
            PersonnelName = assignment.Personnel?.Name ?? "",
            PersonnelRank = assignment.Personnel?.Rank ?? "",
            AssignedAt = assignment.AssignedAt,
            UnassignedAt = assignment.UnassignedAt,
            IsActive = assignment.IsActive
        };

        return CreatedAtAction(nameof(GetVehicleAssignments), new { vehicleId = assignment.VehicleId }, assignmentDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveVehicleAssignment(int id)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (!userAgencyId.HasValue)
        {
            return Unauthorized("User agency information not found");
        }

        var assignment = await _context.VehicleAssignments
            .Include(va => va.Vehicle)
                .ThenInclude(v => v.Station)
            .FirstOrDefaultAsync(va => va.Id == id);
        
        if (assignment == null)
            return NotFound();

        // Validate user can access this assignment (vehicle belongs to their agency)
        if (assignment.Vehicle?.Station?.AgencyId != userAgencyId.Value)
        {
            return Forbid("You do not have permission to modify this assignment");
        }

        var personnelId = assignment.PersonnelId;
        var stationId = assignment.Vehicle!.StationId;

        assignment.IsActive = false;
        assignment.UnassignedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send real-time notification
        await _realTimeService.BroadcastVehicleAssignmentChanged(
            assignment.VehicleId,
            personnelId,
            "unassigned",
            stationId
        );

        return NoContent();
    }
}