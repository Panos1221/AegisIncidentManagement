using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IncidentManagementDbContext _context;

    public UsersController(IncidentManagementDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery] int? stationId = null)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        if (userAgencyId == null)
        {
            return Unauthorized("User agency information not found");
        }

        var query = _context.Users
            .Include(u => u.Station)
            .Include(u => u.Agency)
            .Where(u => u.AgencyId == userAgencyId.Value)
            .AsQueryable();

        if (stationId.HasValue)
            query = query.Where(u => u.StationId == stationId.Value);

        var users = await query.ToListAsync();

        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            SupabaseUserId = u.SupabaseUserId,
            Email = u.Email,
            Name = u.Name,
            Role = u.Role,
            AgencyId = u.AgencyId,
            AgencyName = u.Agency.Name,
            StationId = u.StationId,
            StationName = u.Station?.Name,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        });

        return Ok(userDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Station)
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        var userDto = new UserDto
        {
            Id = user.Id,
            SupabaseUserId = user.SupabaseUserId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            AgencyId = user.AgencyId,
            AgencyName = user.Agency?.Name,
            StationId = user.StationId,
            StationName = user.Station?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    [HttpGet("by-supabase/{supabaseUserId}")]
    public async Task<ActionResult<UserDto>> GetUserBySupabaseId(string supabaseUserId)
    {
        var user = await _context.Users
            .Include(u => u.Station)
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.SupabaseUserId == supabaseUserId);

        if (user == null)
            return NotFound();

        var userDto = new UserDto
        {
            Id = user.Id,
            SupabaseUserId = user.SupabaseUserId,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            AgencyId = user.AgencyId,
            AgencyName = user.Agency.Name,
            StationId = user.StationId,
            StationName = user.Station?.Name,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)
    {
        var user = new User
        {
            SupabaseUserId = createUserDto.SupabaseUserId,
            Email = createUserDto.Email,
            Password = createUserDto.Password, // In production, hash this password
            Name = createUserDto.Name,
            Role = createUserDto.Role,
            AgencyId = createUserDto.AgencyId,
            StationId = createUserDto.StationId
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Reload user with Agency information
        var createdUser = await _context.Users
            .Include(u => u.Agency)
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        var userDto = new UserDto
        {
            Id = createdUser!.Id,
            SupabaseUserId = createdUser.SupabaseUserId,
            Email = createdUser.Email,
            Name = createdUser.Name,
            Role = createdUser.Role,
            AgencyId = createdUser.AgencyId,
            AgencyName = createdUser.Agency.Name,
            StationId = createdUser.StationId,
            StationName = createdUser.Station?.Name,
            IsActive = createdUser.IsActive,
            CreatedAt = createdUser.CreatedAt
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userDto);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] dynamic updates)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Update properties dynamically
        var properties = updates.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(updates);
            if (value != null)
            {
                var userProperty = typeof(User).GetProperty(property.Name);
                if (userProperty != null && userProperty.CanWrite)
                {
                    userProperty.SetValue(user, value);
                }
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}