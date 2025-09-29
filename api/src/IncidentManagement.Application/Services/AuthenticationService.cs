using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace IncidentManagement.Application.Services;

public interface IAuthenticationService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    Task<bool> ValidateUserCredentialsAsync(string email, string password);
    string GenerateJwtToken(User user);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IncidentManagementDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthenticationService(IncidentManagementDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users
            .Include(u => u.Agency)
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

        if (user == null || user.Password != loginDto.Password)
        {
            return null; // Invalid credentials
        }

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role,
            AgencyId = user.AgencyId,
            AgencyName = user.Agency.Name,
            StationId = user.StationId,
            StationName = user.Station?.Name,
            IsActive = user.IsActive,
            Token = token
        };
    }

    public async Task<bool> ValidateUserCredentialsAsync(string email, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        return user != null && user.Password == password;
    }

    public string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        var issuer = jwtSettings["Issuer"] ?? "IncidentManagementSystem";
        var audience = jwtSettings["Audience"] ?? "IncidentManagementSystem";
        var expiryInHours = int.Parse(jwtSettings["ExpiryInHours"] ?? "24");
        var expiryInMinutes = expiryInHours * 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("AgencyId", user.AgencyId.ToString()),
            new Claim("StationId", user.StationId?.ToString() ?? "")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryInMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}