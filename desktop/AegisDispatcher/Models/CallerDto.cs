namespace AegisDispatcher.Models
{
    public class CallerDto
    {
        public string? Name { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? CalledAt { get; set; }
        public string? Notes { get; set; }
    }
}
