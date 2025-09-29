namespace StationConfigurator.Models;

public class AgencyDto
{
    public int Id { get; set; }
    public AgencyType Type { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class StationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class CreateStationDto
{
    public string Name { get; set; } = "";
    public int AgencyId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

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

public class PersonnelDto
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Name { get; set; } = "";
    public string Rank { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; }
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public StationDto? Station { get; set; }
}

public class CreatePersonnelDto
{
    public int StationId { get; set; }
    public int AgencyId { get; set; }
    public string Name { get; set; } = "";
    public string Rank { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class VehicleDto
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Callsign { get; set; } = "";
    public string Type { get; set; } = "";
    public VehicleStatus Status { get; set; }
    public string PlateNumber { get; set; } = "";
    public double? WaterLevelLiters { get; set; }
    public double? WaterCapacityLiters { get; set; }
    public double? FoamLevelLiters { get; set; }
    public int? FuelLevelPercent { get; set; }
    public double? BatteryVoltage { get; set; }
    public double? PumpPressureKPa { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastTelemetryAt { get; set; }
    public StationDto? Station { get; set; }
}

public class CreateVehicleDto
{
    public int StationId { get; set; }
    public string Callsign { get; set; } = "";
    public string Type { get; set; } = "";
    public string PlateNumber { get; set; } = "";
    public double? WaterCapacityLiters { get; set; }
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

public class FireStationSimpleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public double Area { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}