namespace IncidentManagement.Application.Services;

public interface IHospitalDataService
{
    Task LoadHospitalDataAsync();
    Task<bool> IsDataLoadedAsync();
}