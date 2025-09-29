namespace AegisDispatcher.Models
{
    public class LoginResponse
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public int AgencyId { get; set; }
        public string AgencyName { get; set; } = string.Empty;
        public int? StationId { get; set; }
        public string? StationName { get; set; }
        public bool IsActive { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
