namespace IncidentManagement.Application.DTOs;

public class PersonnelDto
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public string Name { get; set; } = "";
    public string Rank { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; }
    
    // Agency information for easier filtering
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    
    // Station information for agency filtering
    public StationDto? Station { get; set; }
}

public class CreatePersonnelDto
{
    public int StationId { get; set; }
    public int AgencyId { get; set; }
    public string Name { get; set; } = "";
    public string Rank { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdatePersonnelDto
{
    public string? Name { get; set; }
    public string? Rank { get; set; }
    public string? BadgeNumber { get; set; }
    public bool? IsActive { get; set; }
}