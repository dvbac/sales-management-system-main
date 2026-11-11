using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SalesManagement.BLL.Interfaces;
using SalesManagement.Web.Hubs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<SystemHub> _hubContext;

    public SystemNotificationsController(INotificationService notificationService, IHubContext<SystemHub> hubContext)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
            return Ok(new { data = notifications, unreadCount = unreadCount });
        }
        return Unauthorized();
    }

    [HttpPost("mark-read/{id}")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var success = await _notificationService.MarkAsReadAsync(id);
        return Ok(new { success = success });
    }

    [HttpPost("mark-all-read")]
    [Authorize]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var success = await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { success = success });
        }
        return Unauthorized();
    }

    [HttpPost("trigger-test")]
    public async Task<IActionResult> TriggerTestNotification(int? userId, string title, string message)
    {
        // For testing SignalR easily via Postman or browser Console
        var notification = await _notificationService.CreateNotificationAsync(userId, title, message, "System", "/");
        
        // Broadcast over Hub
        if (userId.HasValue)
        {
            // Usually we map connectionId to userId, for simplicity let's just trigger all clients or target
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Title, notification.Message);
        }
        else 
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification.Title, notification.Message);
        }
        
        return Ok(notification);
    }
}
