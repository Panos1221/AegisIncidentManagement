namespace AegisDispatcher.Models
{
    public class Ship
    {
        public int Mmsi { get; set; }
        public string? Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Course { get; set; }
        public DateTime LastUpdate { get; set; }
        public string? VesselType { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
    }
}