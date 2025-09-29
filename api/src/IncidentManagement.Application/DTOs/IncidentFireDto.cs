namespace IncidentManagement.Application.DTOs;

public class IncidentFireDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public string? BurnedArea { get; set; }
    public string? BurnedItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateIncidentFireDto
{
    public string? BurnedArea { get; set; }
    public string? BurnedItems { get; set; }
}

public class UpdateIncidentFireDto : CreateIncidentFireDto
{
}