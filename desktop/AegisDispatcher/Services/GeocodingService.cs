using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace AegisDispatcher.Services
{
    public class GeocodingService : IGeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;
        private readonly string _nominatimBaseUrl = "https://nominatim.openstreetmap.org";

        public GeocodingService(HttpClient httpClient, ILoggingService loggingService)
        {
            _httpClient = httpClient;
            _loggingService = loggingService;
            
            // Set user agent as required by Nominatim
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AegisDispatcher/1.0 (Emergency Response System)");
        }

        public async Task<List<GeocodingResult>> SearchAddressAsync(string query, int maxResults = 5)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return new List<GeocodingResult>();

                var encodedQuery = HttpUtility.UrlEncode(query);
                var url = $"{_nominatimBaseUrl}/search?q={encodedQuery}&format=json&addressdetails=1&limit={maxResults}&countrycodes=gr";

                _loggingService.LogDebug($"Geocoding search request: {url}");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var nominatimResults = JsonSerializer.Deserialize<List<NominatimSearchResult>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                var results = nominatimResults?.Select(ConvertToGeocodingResult).ToList() ?? new List<GeocodingResult>();
                
                _loggingService.LogDebug($"Geocoding search returned {results.Count} results for query: {query}");
                return results;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error during geocoding search for query: {query}");
                return new List<GeocodingResult>();
            }
        }

        public async Task<GeocodingResult?> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"{_nominatimBaseUrl}/reverse?lat={latitude:F6}&lon={longitude:F6}&format=json&addressdetails=1";

                _loggingService.LogDebug($"Reverse geocoding request: {url}");

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonContent = await response.Content.ReadAsStringAsync();
                var nominatimResult = JsonSerializer.Deserialize<NominatimReverseResult>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                if (nominatimResult != null)
                {
                    var result = ConvertToGeocodingResult(nominatimResult);
                    _loggingService.LogDebug($"Reverse geocoding successful for coordinates: {latitude:F6}, {longitude:F6}");
                    return result;
                }

                return null;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"Error during reverse geocoding for coordinates: {latitude:F6}, {longitude:F6}");
                return null;
            }
        }

        private GeocodingResult ConvertToGeocodingResult(NominatimSearchResult nominatim)
        {
            return new GeocodingResult
            {
                DisplayName = nominatim.DisplayName ?? "",
                Latitude = double.Parse(nominatim.Lat ?? "0"),
                Longitude = double.Parse(nominatim.Lon ?? "0"),
                HouseNumber = nominatim.Address?.HouseNumber,
                Street = nominatim.Address?.Road ?? nominatim.Address?.Street,
                City = nominatim.Address?.City ?? nominatim.Address?.Town ?? nominatim.Address?.Village,
                Region = nominatim.Address?.State ?? nominatim.Address?.Region,
                PostalCode = nominatim.Address?.Postcode,
                Country = nominatim.Address?.Country,
                CountryCode = nominatim.Address?.CountryCode,
                Importance = nominatim.Importance ?? 0,
                Type = nominatim.Type ?? "",
                Class = nominatim.Class ?? ""
            };
        }

        private GeocodingResult ConvertToGeocodingResult(NominatimReverseResult nominatim)
        {
            return new GeocodingResult
            {
                DisplayName = nominatim.DisplayName ?? "",
                Latitude = double.Parse(nominatim.Lat ?? "0"),
                Longitude = double.Parse(nominatim.Lon ?? "0"),
                HouseNumber = nominatim.Address?.HouseNumber,
                Street = nominatim.Address?.Road ?? nominatim.Address?.Street,
                City = nominatim.Address?.City ?? nominatim.Address?.Town ?? nominatim.Address?.Village,
                Region = nominatim.Address?.State ?? nominatim.Address?.Region,
                PostalCode = nominatim.Address?.Postcode,
                Country = nominatim.Address?.Country,
                CountryCode = nominatim.Address?.CountryCode,
                Importance = nominatim.Importance ?? 0,
                Type = nominatim.Type ?? "",
                Class = nominatim.Class ?? ""
            };
        }
    }

    // Nominatim API response models
    internal class NominatimSearchResult
    {
        public string? DisplayName { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }
        public double? Importance { get; set; }
        public string? Type { get; set; }
        public string? Class { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    internal class NominatimReverseResult
    {
        public string? DisplayName { get; set; }
        public string? Lat { get; set; }
        public string? Lon { get; set; }
        public double? Importance { get; set; }
        public string? Type { get; set; }
        public string? Class { get; set; }
        public NominatimAddress? Address { get; set; }
    }

    internal class NominatimAddress
    {
        public string? HouseNumber { get; set; }
        public string? Road { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? State { get; set; }
        public string? Region { get; set; }
        public string? Postcode { get; set; }
        public string? Country { get; set; }
        public string? CountryCode { get; set; }
    }
}