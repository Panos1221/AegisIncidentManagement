namespace IncidentManagement.Application.Services.Vessels;

public interface IAisStreamService
{
    IEnumerable<Ship> GetShips();
}
