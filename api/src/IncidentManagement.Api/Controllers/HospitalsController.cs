using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.OutputCaching;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HospitalsController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<HospitalsController> _logger;
    private readonly IMemoryCache _cache;

    public HospitalsController(
        IncidentManagementDbContext context, 
        ILogger<HospitalsController> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Get count of loaded hospitals for testing purposes
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<object>> GetHospitalCount()
    {
        try
        {
            var hospitalCount = await _context.Hospitals.CountAsync();
            var ekabHospitalCount = await _context.Hospitals
                .Where(h => h.AgencyCode == "EKAB")
                .CountAsync();
            
            return Ok(new 
            { 
                TotalHospitalCount = hospitalCount,
                EkabHospitalCount = ekabHospitalCount,
                Message = hospitalCount > 0 ? "Hospital data loaded successfully" : "No hospital data found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hospital count");
            return StatusCode(500, "Error retrieving hospital data");
        }
    }

    /// <summary>
    /// Get basic hospital information
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<HospitalDto>>> GetHospitals(
        [FromQuery] int limit = 50,
        [FromQuery] string? agencyCode = null)
    {
        try
        {
            var query = _context.Hospitals.AsQueryable();
            
            // Filter by agency code if specified (default to EKAB for hospitals)
            if (!string.IsNullOrEmpty(agencyCode))
            {
                query = query.Where(h => h.AgencyCode == agencyCode);
            }
            else
            {
                // Default to EKAB hospitals
                query = query.Where(h => h.AgencyCode == "EKAB");
            }
            
            var hospitals = await query
                .Take(limit)
                .Select(h => new HospitalDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    Region = h.Region,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();
            
            return Ok(hospitals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hospitals");
            return StatusCode(500, "Error retrieving hospital data");
        }
    }

    /// <summary>
    /// Get hospital by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<HospitalDto>> GetHospital(int id)
    {
        try
        {
            var hospital = await _context.Hospitals
                .Where(h => h.Id == id)
                .Select(h => new HospitalDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    Region = h.Region,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    CreatedAt = h.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (hospital == null)
            {
                return NotFound($"Hospital with ID {id} not found");
            }

            return Ok(hospital);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hospital with ID {HospitalId}", id);
            return StatusCode(500, "Error retrieving hospital data");
        }
    }

    /// <summary>
    /// Get hospitals within a geographic bounding box
    /// </summary>
    [HttpGet("bounds")]
    [OutputCache(Duration = 300)] // Cache for 5 minutes
    public async Task<ActionResult<List<HospitalDto>>> GetHospitalsInBounds(
        [FromQuery] double minLat,
        [FromQuery] double maxLat,
        [FromQuery] double minLng,
        [FromQuery] double maxLng,
        [FromQuery] string? agencyCode = null)
    {
        try
        {
            // Create cache key based on parameters
            var cacheKey = $"hospitals_bounds_{minLat}_{maxLat}_{minLng}_{maxLng}_{agencyCode ?? "EKAB"}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out List<HospitalDto>? cachedHospitals))
            {
                _logger.LogDebug("Cache hit for hospitals in bounds");
                return Ok(cachedHospitals);
            }

            var query = _context.Hospitals.AsQueryable();
            
            // Filter by agency code (default to EKAB)
            if (!string.IsNullOrEmpty(agencyCode))
            {
                query = query.Where(h => h.AgencyCode == agencyCode);
            }
            else
            {
                query = query.Where(h => h.AgencyCode == "EKAB");
            }
            
            // Filter by geographic bounds
            var hospitals = await query
                .Where(h => h.Latitude >= minLat && h.Latitude <= maxLat &&
                           h.Longitude >= minLng && h.Longitude <= maxLng)
                .Select(h => new HospitalDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    Address = h.Address,
                    City = h.City,
                    Region = h.Region,
                    Latitude = h.Latitude,
                    Longitude = h.Longitude,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            // Cache the results for 5 minutes
            _cache.Set(cacheKey, hospitals, TimeSpan.FromMinutes(5));
            
            return Ok(hospitals);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving hospitals in bounds");
            return StatusCode(500, "Error retrieving hospital data");
        }
    }
}