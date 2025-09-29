namespace AegisDispatcher.Models
{
    public class IncidentTypesByAgency
    {
        public string AgencyName { get; set; } = string.Empty;
        public List<IncidentTypeCategory> Categories { get; set; } = new();
    }
}
