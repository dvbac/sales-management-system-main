using System.Collections.Generic;
using System.Threading.Tasks;
using SalesManagement.BLL.DTOs;

namespace SalesManagement.BLL.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 20);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<NotificationDto> CreateNotificationAsync(int? userId, string title, string message, string type, string actionUrl);
}
