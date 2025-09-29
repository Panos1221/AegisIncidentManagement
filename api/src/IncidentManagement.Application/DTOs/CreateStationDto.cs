using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Application.DTOs;

public class CreateStationDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [Required]
    public int AgencyId { get; set; }

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }
}
