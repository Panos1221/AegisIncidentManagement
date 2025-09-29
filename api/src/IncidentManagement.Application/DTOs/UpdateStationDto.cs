using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Application.DTOs;

public class UpdateStationDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }
}
