using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Api.Controllers;
using IncidentManagement.Application.Services;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PoliceStationsController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly IStationService _stationService;
    private readonly ILogger<PoliceStationsController> _logger;

    public PoliceStationsController(
        IncidentManagementDbContext context,
        IStationService stationService,
        ILogger<PoliceStationsController> logger)
    {
        _context = context;
        _stationService = stationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PoliceStationDto>>> GetPoliceStations()
    {
        try
        {
            var stations = await _context.PoliceStations
                .Select(s => new PoliceStationDto
                {
                    Id = s.Id,
                    Gid = s.Gid,
                    OriginalId = s.OriginalId,
                    Name = s.Name,
                    Address = s.Address,
                    Sinoikia = s.Sinoikia,
                    Diam = s.Diam,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(stations);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving police stations: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PoliceStationDto>> GetPoliceStation(int id)
    {
        try
        {
            var station = await _context.PoliceStations
                .Where(s => s.Id == id)
                .Select(s => new PoliceStationDto
                {
                    Id = s.Id,
                    Gid = s.Gid,
                    OriginalId = s.OriginalId,
                    Name = s.Name,
                    Address = s.Address,
                    Sinoikia = s.Sinoikia,
                    Diam = s.Diam,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (station == null)
            {
                return NotFound($"Police station with ID {id} not found");
            }

            return Ok(station);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving police station: {ex.Message}");
        }
    }

    /// <summary>
    /// Get all stations for filtering purposes (filtered by user's agency)
    /// </summary>
    [HttpGet("stations")]
    public async Task<ActionResult<List<PoliceStationDto>>> GetStations()
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var stations = await _stationService.GetStationsByAgencyAsync(userAgencyId.Value);

            var policeStationDtos = stations.Select(s => new PoliceStationDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = "", // Station entity doesn't have address
                Sinoikia = "", // Station entity doesn't have sinoikia
                Diam = "", // Station entity doesn't have diam
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                CreatedAt = DateTime.UtcNow // Station entity doesn't have CreatedAt
            }).ToList();

            return Ok(policeStationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stations");
            return StatusCode(500, "Error retrieving station data");
        }
    }
}