namespace IncidentManagement.Application.Configuration;

public class PoliceStationDataOptions
{
    public const string SectionName = "PoliceStationData";

    public string GeoJsonFilePath { get; set; } = "";
    public bool EnableDataLoading { get; set; } = false;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}