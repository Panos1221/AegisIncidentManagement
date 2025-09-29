using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string SupabaseUserId { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Name { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.Member;
    public int AgencyId { get; set; }
    public Agency Agency { get; set; } = null!;
    public int? StationId { get; set; }
    public Station? Station { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}