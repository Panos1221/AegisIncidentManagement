namespace IncidentManagement.Application.DTOs;

public class IncidentTypeCategoryDto
{
    public string CategoryKey { get; set; } = "";
    public string CategoryNameEl { get; set; } = "";
    public string CategoryNameEn { get; set; } = "";
    public List<IncidentTypeSubcategoryDto> Subcategories { get; set; } = new();
}

public class IncidentTypeSubcategoryDto
{
    public string SubcategoryNameEl { get; set; } = "";
    public string SubcategoryNameEn { get; set; } = "";
}

public class IncidentTypesByAgencyDto
{
    public string AgencyName { get; set; } = "";
    public List<IncidentTypeCategoryDto> Categories { get; set; } = new();
}