namespace AegisDispatcher.Models
{
    public class Caller
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public string? Name { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? CalledAt { get; set; }
        public string? Notes { get; set; }
    }
}
