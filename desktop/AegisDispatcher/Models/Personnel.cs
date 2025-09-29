namespace AegisDispatcher.Models
{
    public class Personnel
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string? BadgeNumber { get; set; }
        public bool IsActive { get; set; }
        public int AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
    }
}
