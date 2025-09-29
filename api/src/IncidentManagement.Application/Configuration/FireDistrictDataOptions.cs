namespace IncidentManagement.Application.Configuration;

public class FireDistrictDataOptions
{
    public const string SectionName = "FireDistrictData";

    public string JsonFilePath { get; set; } = string.Empty;
    public bool EnableDataLoading { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
}