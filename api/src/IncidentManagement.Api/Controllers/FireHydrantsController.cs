using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.Services;
using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FireHydrantsController : BaseController
{
    private readonly IFireHydrantDataService _fireHydrantDataService;
    private readonly ILogger<FireHydrantsController> _logger;

    public FireHydrantsController(
        IFireHydrantDataService fireHydrantDataService,
        ILogger<FireHydrantsController> logger)
    {
        _fireHydrantDataService = fireHydrantDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get all fire hydrants - accessible only to Fire Department members
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FireHydrantDto>>> GetFireHydrants()
    {
        try
        {
            // Check if user is a Fire Department member (either Member or Dispatcher)
            var userRole = GetCurrentUserRole();
            var isMember = IsMember();
            var isDispatcher = IsDispatcher();
            
            _logger.LogInformation("Fire hydrants access attempt - Role: {Role}, IsMember: {IsMember}, IsDispatcher: {IsDispatcher}", 
                userRole, isMember, isDispatcher);
            
            if (string.IsNullOrEmpty(userRole) || (!isMember && !isDispatcher))
            {
                _logger.LogWarning("Unauthorized access attempt to fire hydrants by user with role: {Role}", userRole);
                return Forbid("Access denied. Only Fire Department members can access fire hydrant data.");
            }

            var fireHydrants = await _fireHydrantDataService.GetFireHydrantsAsync();
            
            _logger.LogInformation("Retrieved {Count} fire hydrants for user {UserId}", 
                fireHydrants.Count(), GetCurrentUserId());
            
            return Ok(fireHydrants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire hydrants");
            return StatusCode(500, "An error occurred while retrieving fire hydrants");
        }
    }

    /// <summary>
    /// Get fire hydrant by ID - accessible only to Fire Department members
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FireHydrantDto>> GetFireHydrant(int id)
    {
        try
        {
            // Check if user is a Fire Department member (either Member or Dispatcher)
            var userRole = GetCurrentUserRole();
            if (string.IsNullOrEmpty(userRole) || (!IsMember() && !IsDispatcher()))
            {
                _logger.LogWarning("Unauthorized access attempt to fire hydrant {Id} by user with role: {Role}", id, userRole);
                return Forbid("Access denied. Only Fire Department members can access fire hydrant data.");
            }

            var fireHydrant = await _fireHydrantDataService.GetFireHydrantByIdAsync(id);
            
            if (fireHydrant == null)
            {
                _logger.LogWarning("Fire hydrant with ID {Id} not found", id);
                return NotFound($"Fire hydrant with ID {id} not found");
            }

            _logger.LogInformation("Retrieved fire hydrant {Id} for user {UserId}", id, GetCurrentUserId());
            
            return Ok(fireHydrant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire hydrant with ID {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the fire hydrant");
        }
    }

    /// <summary>
    /// Get count of fire hydrants for testing purposes - accessible only to Fire Department members
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<object>> GetFireHydrantCount()
    {
        try
        {
            // Check if user is a Fire Department member (either Member or Dispatcher)
            var userRole = GetCurrentUserRole();
            if (string.IsNullOrEmpty(userRole) || (!IsMember() && !IsDispatcher()))
            {
                _logger.LogWarning("Unauthorized access attempt to fire hydrant count by user with role: {Role}", userRole);
                return Forbid("Access denied. Only Fire Department members can access fire hydrant data.");
            }

            var fireHydrants = await _fireHydrantDataService.GetFireHydrantsAsync();
            var count = fireHydrants.Count();
            
            _logger.LogInformation("Retrieved fire hydrant count ({Count}) for user {UserId}", count, GetCurrentUserId());
            
            return Ok(new { FireHydrantCount = count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire hydrant count");
            return StatusCode(500, "An error occurred while retrieving fire hydrant count");
        }
    }
}