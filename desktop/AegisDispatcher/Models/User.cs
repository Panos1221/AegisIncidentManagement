namespace AegisDispatcher.Models
{
    public class User
    {
        public int Id { get; set; }
        public string SupabaseUserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int? AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public int? StationId { get; set; }
        public string? StationName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
