using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StationsController : BaseController
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    /// <summary>
    /// Get all stations for the current user's agency
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StationDto>>> GetStations()
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var stations = await _stationService.GetStationsByAgencyAsync(userAgencyId.Value);
            return Ok(stations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving stations", error = ex.Message });
        }
    }

    /// <summary>
    /// Get station by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<StationDto>> GetStation(int id)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var station = await _stationService.GetStationByIdAsync(id, userAgencyId);

            if (station == null)
                return NotFound();

            return Ok(station);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving station", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new station
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<StationDto>> CreateStation(CreateStationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var station = await _stationService.CreateStationAsync(dto);
            return CreatedAtAction(nameof(GetStation), new { id = station.Id }, station);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error creating station", error = ex.Message });
        }
    }

    /// <summary>
    /// Update a station
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<StationDto>> UpdateStation(int id, UpdateStationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userAgencyId = GetCurrentUserAgencyId();
            var station = await _stationService.UpdateStationAsync(id, dto, userAgencyId);
            return Ok(station);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error updating station", error = ex.Message });
        }
    }
}