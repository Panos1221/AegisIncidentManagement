using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShipsController : ControllerBase
{
    private readonly IShipStore _store;

    public ShipsController(IShipStore store) => _store = store;

    [HttpGet]
    public IActionResult GetShips()
    {
        var ships = _store.GetShips();
        return Ok(ships);
    }
}