namespace IncidentManagement.Domain.Entities;

public class VehicleTelemetry
{
    public long Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    public double? WaterLevelLiters { get; set; }
    public int? FuelLevelPercent { get; set; }
    public double? BatteryVoltage { get; set; }
    public double? PumpPressureKPa { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}