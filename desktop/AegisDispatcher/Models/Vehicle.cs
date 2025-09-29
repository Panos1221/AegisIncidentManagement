namespace AegisDispatcher.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string Callsign { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public int? WaterLevelLiters { get; set; }
        public int? WaterCapacityLiters { get; set; }
        public int? FoamLevelLiters { get; set; }
        public int? FuelLevelPercent { get; set; }
        public double? BatteryVoltage { get; set; }
        public double? PumpPressureKPa { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime? LastTelemetryAt { get; set; }
    }
}
