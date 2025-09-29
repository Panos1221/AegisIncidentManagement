using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationAssignmentController : BaseController
{
    private readonly IStationAssignmentService _stationAssignmentService;

    public StationAssignmentController(IStationAssignmentService stationAssignmentService)
    {
        _stationAssignmentService = stationAssignmentService;
    }

    [HttpPost("find-by-location")]
    public async Task<IActionResult> FindStationByLocation([FromBody] StationAssignmentRequestDto request)
    {
        if (request.Latitude == 0 || request.Longitude == 0)
        {
            return BadRequest("Invalid coordinates provided");
        }

        if (string.IsNullOrEmpty(request.AgencyType))
        {
            return BadRequest("Agency type is required");
        }

        var result = await _stationAssignmentService.FindStationByLocationAsync(request);
        if (result == null)
        {
            return NotFound("No station found for the specified location and agency type");
        }

        return Ok(result);
    }

    [HttpPost("fire-station/find-by-location")]
    public async Task<IActionResult> FindFireStationByLocation([FromBody] LocationDto location)
    {
        if (location.Latitude == 0 || location.Longitude == 0)
        {
            return BadRequest("Invalid coordinates provided");
        }

        var fireStation = await _stationAssignmentService.FindFireStationByLocationAsync(location.Latitude, location.Longitude);
        if (fireStation == null)
        {
            return NotFound("No fire station found for this location");
        }

        return Ok(fireStation);
    }

    [HttpPost("coastguard-station/find-nearest")]
    public async Task<IActionResult> FindNearestCoastGuardStation([FromBody] LocationDto location)
    {
        if (location.Latitude == 0 || location.Longitude == 0)
        {
            return BadRequest("Invalid coordinates provided");
        }

        var station = await _stationAssignmentService.FindNearestCoastGuardStationAsync(location.Latitude, location.Longitude);
        if (station == null)
        {
            return NotFound("No coast guard station found");
        }

        return Ok(station);
    }

    [HttpPost("police-station/find-nearest")]
    public async Task<IActionResult> FindNearestPoliceStation([FromBody] LocationDto location)
    {
        if (location.Latitude == 0 || location.Longitude == 0)
        {
            return BadRequest("Invalid coordinates provided");
        }

        var station = await _stationAssignmentService.FindNearestPoliceStationAsync(location.Latitude, location.Longitude);
        if (station == null)
        {
            return NotFound("No police station found");
        }

        return Ok(station);
    }

    [HttpPost("hospital/find-nearest")]
    public async Task<IActionResult> FindNearestHospital([FromBody] LocationDto location)
    {
        if (location.Latitude == 0 || location.Longitude == 0)
        {
            return BadRequest("Invalid coordinates provided");
        }

        var hospital = await _stationAssignmentService.FindNearestHospitalAsync(location.Latitude, location.Longitude);
        if (hospital == null)
        {
            return NotFound("No hospital found");
        }

        return Ok(hospital);
    }
}