using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using IncidentManagement.Application.Services;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Api.Controllers
{
    [Route("api/fire-districts")]
    [ApiController]
    public class FireDistrictsController : ControllerBase
    {
        private readonly ILogger<FireDistrictsController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;
        private readonly IGeographicService _geographicService;
        private const string CACHE_KEY = "fire_districts_data";

        public FireDistrictsController(
            ILogger<FireDistrictsController> logger,
            IMemoryCache cache,
            IWebHostEnvironment environment,
            IGeographicService geographicService)
        {
            _logger = logger;
            _cache = cache;
            _environment = environment;
            _geographicService = geographicService;
        }

        [HttpGet]
        [ResponseCache(Duration = 2592000, Location = ResponseCacheLocation.Any, NoStore = false)]
        public async Task<ActionResult> GetFireDistricts()
        {
            try
            {
                _logger.LogInformation("API request received for fire districts");

                // Try to get data from cache first
                if (_cache.TryGetValue(CACHE_KEY, out object? cachedData))
                {
                    _logger.LogInformation("Returning cached fire districts data");
                    return Ok(cachedData);
                }

                // Load data from file if not in cache
                var filePath = Path.Combine(_environment.ContentRootPath, "Data", "fire_depts_districts.json");
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogError($"Fire districts file not found at: {filePath}");
                    return NotFound("Fire districts data not available");
                }

                _logger.LogInformation($"Loading fire districts from file: {filePath}");
                var jsonContent = await System.IO.File.ReadAllTextAsync(filePath);

                // Parse and validate JSON
                var districtsData = JsonSerializer.Deserialize<object>(jsonContent);
                if (districtsData == null)
                {
                    _logger.LogError("Failed to parse fire districts JSON data");
                    return StatusCode(500, "Error parsing districts data");
                }

                // Cache the data
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    Priority = CacheItemPriority.Normal,
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Cache for 24 hours
                };
                _cache.Set(CACHE_KEY, districtsData, cacheOptions);

                return Ok(districtsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving fire districts data");
                return StatusCode(500, "Internal server error while retrieving districts data");
            }
        }

        /// <summary>
        /// Find the responsible fire district/station for given coordinates
        /// </summary>
        [HttpPost("find-responsible-district")]
        public async Task<ActionResult<ResponsibleDistrictDto>> FindResponsibleDistrict([FromBody] FindDistrictByLocationDto locationDto)
        {
            try
            {
                if (locationDto.Latitude < -90 || locationDto.Latitude > 90)
                {
                    return BadRequest("Latitude must be between -90 and 90 degrees");
                }

                if (locationDto.Longitude < -180 || locationDto.Longitude > 180)
                {
                    return BadRequest("Longitude must be between -180 and 180 degrees");
                }

                _logger.LogInformation("Finding responsible district for coordinates: Lat={Latitude}, Lon={Longitude}", 
                    locationDto.Latitude, locationDto.Longitude);

                // Use the geographic service to find the responsible station
                var station = await _geographicService.FindStationByCoordinatesAsync(
                    locationDto.Latitude, 
                    locationDto.Longitude);

                if (station == null)
                {
                    _logger.LogWarning("No responsible district found for coordinates: Lat={Latitude}, Lon={Longitude}", 
                        locationDto.Latitude, locationDto.Longitude);
                    
                    return Ok(new ResponsibleDistrictDto
                    {
                        Found = false,
                        Message = "No responsible fire district found for the given coordinates"
                    });
                }

                var result = new ResponsibleDistrictDto
                {
                    Found = true,
                    StationId = station.Id,
                    StationName = station.Name,
                    Region = station.Region,
                    Area = station.Area,
                    Message = $"Incident should be handled by {station.Name}"
                };

                _logger.LogInformation("Found responsible district: {StationName} for coordinates: Lat={Latitude}, Lon={Longitude}", 
                    station.Name, locationDto.Latitude, locationDto.Longitude);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid coordinates provided: Lat={Latitude}, Lon={Longitude}", 
                    locationDto.Latitude, locationDto.Longitude);
                return BadRequest($"Invalid coordinates: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding responsible district: Lat={Latitude}, Lon={Longitude}", 
                    locationDto.Latitude, locationDto.Longitude);
                return StatusCode(500, "Error performing geographic lookup");
            }
        }

        /// <summary>
        /// Assign incident to responsible district based on coordinates
        /// </summary>
        [HttpPost("assign-incident")]
        public async Task<ActionResult<IncidentAssignmentDto>> AssignIncidentToDistrict([FromBody] AssignIncidentDto assignmentDto)
        {
            try
            {
                _logger.LogInformation("Assigning incident {IncidentId} to responsible district", assignmentDto.IncidentId);

                // Find responsible district
                var station = await _geographicService.FindStationByCoordinatesAsync(
                    assignmentDto.Latitude, 
                    assignmentDto.Longitude);

                if (station == null)
                {
                    return Ok(new IncidentAssignmentDto
                    {
                        Success = false,
                        IncidentId = assignmentDto.IncidentId,
                        Message = "No responsible fire district found for incident location"
                    });
                }

                // Here you would typically update the incident in your database
                // For now, we'll just return the assignment information
                var result = new IncidentAssignmentDto
                {
                    Success = true,
                    IncidentId = assignmentDto.IncidentId,
                    AssignedStationId = station.Id,
                    AssignedStationName = station.Name,
                    Region = station.Region,
                    Message = $"Incident {assignmentDto.IncidentId} assigned to {station.Name}"
                };

                _logger.LogInformation("Incident {IncidentId} assigned to station {StationName}", 
                    assignmentDto.IncidentId, station.Name);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning incident {IncidentId} to district", assignmentDto.IncidentId);
                return StatusCode(500, "Error assigning incident to district");
            }
        }

        [HttpPost("clear-cache")] // Used for debug
        public ActionResult ClearCache()
        {
            try
            {
                _cache.Remove(CACHE_KEY);
                _logger.LogInformation("Fire districts cache cleared");
                return Ok(new { message = "Cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing fire districts cache");
                return StatusCode(500, "Error clearing cache");
            }
        }

        /// <summary>
        /// Get cache statistics for monitoring
        /// </summary>
        [HttpGet("cache-info")]
        public ActionResult GetCacheInfo()
        {
            try
            {
                var hasCachedData = _cache.TryGetValue(CACHE_KEY, out _);
                
                return Ok(new 
                { 
                    cached = hasCachedData,
                    cacheKey = CACHE_KEY,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache info");
                return StatusCode(500, "Error getting cache info");
            }
        }
    }
}