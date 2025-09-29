namespace AegisDispatcher.Models
{
    public class Assignment
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public ResourceType ResourceType { get; set; }
        public int ResourceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? DispatchedAt { get; set; }
        public DateTime? EnRouteAt { get; set; }
        public DateTime? OnSceneAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
