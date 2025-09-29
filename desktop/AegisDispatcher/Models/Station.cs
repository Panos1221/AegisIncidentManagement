namespace AegisDispatcher.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AgencyId { get; set; }
        public AgencyType AgencyType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
