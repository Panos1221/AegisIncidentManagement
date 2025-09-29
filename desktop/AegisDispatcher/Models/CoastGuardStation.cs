namespace AegisDispatcher.Models
{
    public class CoastGuardStation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NameGr { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? Area { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
        public string? Type { get; set; }
    }
}