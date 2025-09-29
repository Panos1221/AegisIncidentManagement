namespace IncidentManagement.Application.Services;

public interface ICoastGuardStationDataService
{
    Task LoadCoastGuardStationDataAsync();
    Task<bool> IsDataAlreadyLoadedAsync();
}