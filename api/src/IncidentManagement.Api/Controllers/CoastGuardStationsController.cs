using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Api.Controllers;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoastGuardStationsController : BaseController
{
    private readonly IncidentManagementDbContext _context;

    public CoastGuardStationsController(IncidentManagementDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CoastGuardStationDto>>> GetCoastGuardStations()
    {
        try
        {
            var stations = await _context.CoastGuardStations
                .Select(s => new CoastGuardStationDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    NameGr = s.NameGr,
                    Address = s.Address,
                    Area = s.Area,
                    Type = s.Type,
                    Telephone = s.Telephone,
                    Email = s.Email,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Ok(stations);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving coast guard stations: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CoastGuardStationDto>> GetCoastGuardStation(int id)
    {
        try
        {
            var station = await _context.CoastGuardStations
                .Where(s => s.Id == id)
                .Select(s => new CoastGuardStationDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    NameGr = s.NameGr,
                    Address = s.Address,
                    Area = s.Area,
                    Type = s.Type,
                    Telephone = s.Telephone,
                    Email = s.Email,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    CreatedAt = s.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (station == null)
            {
                return NotFound($"Coast guard station with ID {id} not found");
            }

            return Ok(station);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error retrieving coast guard station with ID {id}: {ex.Message}");
        }
    }
}