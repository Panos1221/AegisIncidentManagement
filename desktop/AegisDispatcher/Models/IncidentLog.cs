namespace AegisDispatcher.Models
{
    public class IncidentLog
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public DateTime At { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? By { get; set; }
    }
}
