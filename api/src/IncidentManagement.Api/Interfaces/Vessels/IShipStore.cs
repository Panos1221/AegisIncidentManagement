public interface IShipStore
{
    void UpdateShip(Ship ship);
    IEnumerable<Ship> GetShips();
}

public class ShipStore : IShipStore
{
    private readonly Dictionary<string, Ship> _ships = new();

    public void UpdateShip(Ship ship) => _ships[ship.Mmsi] = ship;

    public IEnumerable<Ship> GetShips() => _ships.Values;
}
