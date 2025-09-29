namespace AegisDispatcher.Models
{
    public class CreateAssignment
    {
        public int IncidentId { get; set; }
        public ResourceType ResourceType { get; set; }
        public int ResourceId { get; set; }
        public int AssignedByUserId { get; set; }
    }
}
