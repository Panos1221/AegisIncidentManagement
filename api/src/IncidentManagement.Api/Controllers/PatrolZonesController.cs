using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatrolZonesController : BaseController
{
    private readonly IPatrolZoneService _patrolZoneService;

    public PatrolZonesController(IPatrolZoneService patrolZoneService)
    {
        _patrolZoneService = patrolZoneService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PatrolZoneDto>>> GetPatrolZones([FromQuery] int? stationId)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            IEnumerable<PatrolZoneDto> patrolZones;
            
            if (stationId.HasValue)
            {
                patrolZones = await _patrolZoneService.GetByStationIdAsync(stationId.Value, userAgencyId.Value);
            }
            else
            {
                patrolZones = await _patrolZoneService.GetAllAsync(userAgencyId.Value);
            }

            return Ok(patrolZones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PatrolZoneDto>> GetPatrolZone(int id)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var patrolZone = await _patrolZoneService.GetByIdAsync(id, userAgencyId.Value);
            if (patrolZone == null)
            {
                return NotFound($"Patrol zone with ID {id} not found");
            }

            return Ok(patrolZone);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<ActionResult<PatrolZoneDto>> CreatePatrolZone([FromBody] CreatePatrolZoneDto createDto)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var userId = GetCurrentUserId();
            
            if (!userAgencyId.HasValue || !userId.HasValue)
            {
                return Unauthorized("User information not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var patrolZone = await _patrolZoneService.CreateAsync(createDto, userAgencyId.Value, userId.Value);
            return CreatedAtAction(nameof(GetPatrolZone), new { id = patrolZone.Id }, patrolZone);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PatrolZoneDto>> UpdatePatrolZone(int id, [FromBody] UpdatePatrolZoneDto updateDto)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var patrolZone = await _patrolZoneService.UpdateAsync(id, updateDto, userAgencyId.Value);
            if (patrolZone == null)
            {
                return NotFound($"Patrol zone with ID {id} not found");
            }

            return Ok(patrolZone);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePatrolZone(int id)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var success = await _patrolZoneService.DeleteAsync(id, userAgencyId.Value);
            if (!success)
            {
                return NotFound($"Patrol zone with ID {id} not found");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost("{id}/assignments")]
    public async Task<ActionResult<PatrolZoneAssignmentDto>> AssignVehicle(int id, [FromBody] CreatePatrolZoneAssignmentDto assignmentDto)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var userId = GetCurrentUserId();
            
            if (!userAgencyId.HasValue || !userId.HasValue)
            {
                return Unauthorized("User information not found");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensure the patrol zone ID matches
            if (assignmentDto.PatrolZoneId != id)
            {
                return BadRequest("Patrol zone ID in URL does not match the assignment data");
            }

            var assignment = await _patrolZoneService.AssignVehicleAsync(assignmentDto, userAgencyId.Value, userId.Value);
            return CreatedAtAction(nameof(GetPatrolZone), new { id = id }, assignment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("assignments/{assignmentId}")]
    public async Task<ActionResult> UnassignVehicle(int assignmentId)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var userId = GetCurrentUserId();
            
            if (!userAgencyId.HasValue || !userId.HasValue)
            {
                return Unauthorized("User information not found");
            }

            var success = await _patrolZoneService.UnassignVehicleAsync(assignmentId, userAgencyId.Value, userId.Value);
            if (!success)
            {
                return NotFound($"Assignment with ID {assignmentId} not found or already inactive");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("{id}/assignments")]
    public async Task<ActionResult<IEnumerable<PatrolZoneAssignmentDto>>> GetActiveAssignments(int id)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var assignments = await _patrolZoneService.GetActiveAssignmentsAsync(id, userAgencyId.Value);
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("vehicles/{vehicleId}/assignment-history")]
    public async Task<ActionResult<IEnumerable<PatrolZoneAssignmentDto>>> GetVehicleAssignmentHistory(int vehicleId)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var assignments = await _patrolZoneService.GetVehicleAssignmentHistoryAsync(vehicleId, userAgencyId.Value);
            return Ok(assignments);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}