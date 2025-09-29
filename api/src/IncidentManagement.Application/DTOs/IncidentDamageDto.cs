namespace IncidentManagement.Application.DTOs;

public class IncidentDamageDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public string? OwnerName { get; set; }
    public string? TenantName { get; set; }
    public decimal? DamageAmount { get; set; }
    public decimal? SavedProperty { get; set; }
    public string? IncidentCause { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateIncidentDamageDto
{
    public string? OwnerName { get; set; }
    public string? TenantName { get; set; }
    public decimal? DamageAmount { get; set; }
    public decimal? SavedProperty { get; set; }
    public string? IncidentCause { get; set; }
}

public class UpdateIncidentDamageDto : CreateIncidentDamageDto
{
}