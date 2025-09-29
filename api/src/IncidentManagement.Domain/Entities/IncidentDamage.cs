namespace IncidentManagement.Domain.Entities;

public class IncidentDamage
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }

    // Property damage information
    public string? OwnerName { get; set; }
    public string? TenantName { get; set; }
    public decimal? DamageAmount { get; set; } // in euros
    public decimal? SavedProperty { get; set; } // in euros
    public string? IncidentCause { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}