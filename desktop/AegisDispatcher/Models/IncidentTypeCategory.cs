namespace AegisDispatcher.Models
{
    public class IncidentTypeCategory
    {
        public string CategoryKey { get; set; } = string.Empty;
        public string CategoryNameEl { get; set; } = string.Empty;
        public string CategoryNameEn { get; set; } = string.Empty;
        public List<IncidentTypeSubcategory> Subcategories { get; set; } = new();
    }
}
