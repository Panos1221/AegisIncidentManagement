using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IncidentManagementDbContext _context;

    public HealthController(IncidentManagementDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetHealth()
    {
        try
        {
            // Test database connection
            var canConnect = await _context.Database.CanConnectAsync();
            
            var health = new
            {
                status = canConnect ? "healthy" : "unhealthy",
                timestamp = DateTime.UtcNow,
                database = canConnect ? "connected" : "disconnected",
                version = "1.0.0"
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "unhealthy",
                timestamp = DateTime.UtcNow,
                database = "error",
                error = ex.Message,
                version = "1.0.0"
            });
        }
    }

    [HttpGet("debug")]
    public async Task<ActionResult<object>> GetDebugInfo()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            var incidentCount = canConnect ? await _context.Incidents.CountAsync() : 0;
            var userCount = canConnect ? await _context.Users.CountAsync() : 0;
            var vehicleCount = canConnect ? await _context.Vehicles.CountAsync() : 0;
            
            var debug = new
            {
                status = "debug",
                timestamp = DateTime.UtcNow,
                database = new
                {
                    connected = canConnect,
                    connectionString = _context.Database.GetConnectionString()?.Substring(0, 50) + "...",
                    provider = _context.Database.ProviderName
                },
                counts = new
                {
                    incidents = incidentCount,
                    users = userCount,
                    vehicles = vehicleCount
                },
                version = "1.0.0"
            };

            return Ok(debug);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                status = "error",
                timestamp = DateTime.UtcNow,
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }
}