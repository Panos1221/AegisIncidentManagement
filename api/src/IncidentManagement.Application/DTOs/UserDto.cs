using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string SupabaseUserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public UserRole Role { get; set; }
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public int? StationId { get; set; }
    public string? StationName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string SupabaseUserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
    public UserRole Role { get; set; }
    public int AgencyId { get; set; }
    public int? StationId { get; set; }
}

public class LoginDto
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Name { get; set; } = "";
    public UserRole Role { get; set; }
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public int? StationId { get; set; }
    public string? StationName { get; set; }
    public bool IsActive { get; set; }
    public string Token { get; set; } = "";
}