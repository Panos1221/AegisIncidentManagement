namespace AegisDispatcher.Services
{
    public interface IGeocodingService
    {
        Task<List<GeocodingResult>> SearchAddressAsync(string query, int maxResults = 5);
        Task<GeocodingResult?> ReverseGeocodeAsync(double latitude, double longitude);
    }

    public class GeocodingResult
    {
        public string DisplayName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? HouseNumber { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
        public double Importance { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
    }
}