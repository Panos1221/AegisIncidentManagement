using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Services;

public interface IFireHydrantDataService
{
    Task<IEnumerable<FireHydrant>> LoadFireHydrantsFromGeoJsonAsync();
    Task<bool> SeedFireHydrantsAsync();
    Task<IEnumerable<FireHydrant>> GetFireHydrantsAsync();
    Task<FireHydrant?> GetFireHydrantByIdAsync(int id);
}