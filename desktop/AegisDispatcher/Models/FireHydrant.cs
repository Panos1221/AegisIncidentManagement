namespace AegisDispatcher.Models
{
    public class FireHydrant
    {
        public int Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? Type { get; set; }
        public int? Pressure { get; set; }
        public bool IsActive { get; set; }
    }
}