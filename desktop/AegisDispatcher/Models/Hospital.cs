namespace AegisDispatcher.Models
{
    public class Hospital
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Telephone { get; set; }
        public string? Type { get; set; }
    }
}