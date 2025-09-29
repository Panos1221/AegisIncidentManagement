using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IncidentManagementDbContext _context;

    public NotificationsController(IncidentManagementDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications([FromQuery] int userId, [FromQuery] bool? isRead = null)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (isRead.HasValue)
            query = query.Where(n => n.IsRead == isRead.Value);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var notificationDtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            IncidentId = n.IncidentId,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });

        return Ok(notificationDtos);
    }

    [HttpPost]
    public async Task<ActionResult<NotificationDto>> CreateNotification(CreateNotificationDto createNotificationDto)
    {
        var notification = new Notification
        {
            UserId = createNotificationDto.UserId,
            Type = createNotificationDto.Type,
            Title = createNotificationDto.Title,
            Message = createNotificationDto.Message,
            IncidentId = createNotificationDto.IncidentId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        var notificationDto = new NotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            IncidentId = notification.IncidentId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        return CreatedAtAction(nameof(GetNotifications), new { userId = notification.UserId }, notificationDto);
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("user/{userId}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }
}