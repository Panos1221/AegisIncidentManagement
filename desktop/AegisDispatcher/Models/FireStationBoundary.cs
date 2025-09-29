namespace AegisDispatcher.Models
{
    public class FireStationBoundary
    {
        public int Id { get; set; }
        public int FireStationId { get; set; }
        public string Name { get; set; } = "";
        public string Region { get; set; } = "";
        public double Area { get; set; }
        public List<List<List<double>>> Coordinates { get; set; } = new();
    }
}