namespace IncidentManagement.Application.Services;

public interface IPoliceStationDataService
{
    /// <summary>
    /// Loads police station data from the GeoJSON file into the database
    /// </summary>
    Task LoadDataAsync();
    
    /// <summary>
    /// Checks if police station data has already been loaded into the database
    /// </summary>
    Task<bool> IsDataAlreadyLoadedAsync();
}