namespace IncidentManagement.Application.Services;

public interface IFireStationDataService
{
    /// <summary>
    /// Loads fire station data from the JSON file into the database
    /// </summary>
    Task LoadFireStationDataAsync();
    
    /// <summary>
    /// Loads simple fire station locations from the JSON file into the database
    /// </summary>
    Task LoadFireStationLocationsAsync();
    
    /// <summary>
    /// Checks if fire station data has already been loaded into the database
    /// </summary>
    Task<bool> IsDataAlreadyLoadedAsync();
}