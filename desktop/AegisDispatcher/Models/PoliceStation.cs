namespace AegisDispatcher.Models
{
    public class PoliceStation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Address { get; set; }
        public string? Sinoikia { get; set; }
        public string? Diam { get; set; }
        public string? Telephone { get; set; }
    }
}