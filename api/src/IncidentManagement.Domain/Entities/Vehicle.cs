using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
    public string Callsign { get; set; } = "";
    public string Type { get; set; } = "FireTruck";
    public VehicleStatus Status { get; set; } = VehicleStatus.Available;
    
    // Vehicle details
    public string PlateNumber { get; set; } = "";
    
    // Telemetry (live snapshot)
    public double? WaterLevelLiters { get; set; }
    public double? WaterCapacityLiters { get; set; }
    public double? FoamLevelLiters { get; set; }
    public int? FuelLevelPercent { get; set; }
    public double? BatteryVoltage { get; set; }
    public double? PumpPressureKPa { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime? LastTelemetryAt { get; set; }
}