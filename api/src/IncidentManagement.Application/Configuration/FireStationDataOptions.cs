namespace IncidentManagement.Application.Configuration;

/// <summary>
/// Configuration options for fire station data loading
/// </summary>
public class FireStationDataOptions
{
    public const string SectionName = "FireStationData";
    
    /// <summary>
    /// Path to the fire station JSON file
    /// </summary>
    public string JsonFilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to enable fire station data loading on startup
    /// </summary>
    public bool EnableDataLoading { get; set; } = true;
    
    /// <summary>
    /// Maximum number of retry attempts for data loading
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;
    
    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;
}