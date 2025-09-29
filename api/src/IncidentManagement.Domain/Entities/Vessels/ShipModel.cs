public class Ship
{
    public string Mmsi { get; set; } = string.Empty;
    public string? Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Speed { get; set; } // knots
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
