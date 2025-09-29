using IncidentManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Callsign { get; set; } = "";
    public string Type { get; set; } = "";
    public VehicleStatus Status { get; set; }
    public string PlateNumber { get; set; } = "";
    
    // Telemetry
    public double? WaterLevelLiters { get; set; }
    public double? WaterCapacityLiters { get; set; }
    public double? FoamLevelLiters { get; set; }
    public int? FuelLevelPercent { get; set; }
    public double? BatteryVoltage { get; set; }
    public double? PumpPressureKPa { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastTelemetryAt { get; set; }
    
    // Station information for agency filtering
    public StationDto? Station { get; set; }
}

public class CreateVehicleDto
{
    [Required(ErrorMessage = "Station ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Station ID must be a positive number")]
    public int StationId { get; set; }
    
    [Required(ErrorMessage = "Callsign is required")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Callsign must be between 1 and 50 characters")]
    public string Callsign { get; set; } = "";
    
    [Required(ErrorMessage = "Type is required")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Type must be between 1 and 100 characters")]
    public string Type { get; set; } = "";
    
    [StringLength(20, ErrorMessage = "Plate number cannot exceed 20 characters")]
    public string PlateNumber { get; set; } = "";
    
    [Range(0, double.MaxValue, ErrorMessage = "Water capacity must be non-negative")]
    public double? WaterCapacityLiters { get; set; }
}

public class UpdateVehicleDto
{
    // Basic vehicle information that can be edited
    [StringLength(50, ErrorMessage = "Callsign cannot exceed 50 characters")]
    public string? Callsign { get; set; }
    
    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
    public string? Type { get; set; }
    
    [StringLength(20, ErrorMessage = "Plate number cannot exceed 20 characters")]
    public string? PlateNumber { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Station ID must be a positive number")]
    public int? StationId { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Water capacity must be non-negative")]
    public double? WaterCapacityLiters { get; set; }
    
    // Status and telemetry (existing fields)
    public VehicleStatus? Status { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Water level must be non-negative")]
    public double? WaterLevelLiters { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Foam level must be non-negative")]
    public double? FoamLevelLiters { get; set; }
    
    [Range(0, 100, ErrorMessage = "Fuel level must be between 0 and 100 percent")]
    public int? FuelLevelPercent { get; set; }
    
    public double? BatteryVoltage { get; set; }
    public double? PumpPressureKPa { get; set; }
    
    [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees")]
    public double? Latitude { get; set; }
    
    [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees")]
    public double? Longitude { get; set; }
}