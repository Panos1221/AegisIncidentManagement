using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public int? IncidentId { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateNotificationDto
{
    public int UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = "";
    public string Message { get; set; } = "";
    public int? IncidentId { get; set; }
}