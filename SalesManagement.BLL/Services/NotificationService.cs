using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.DTOs;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 20)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId || n.UserId == null)
            .OrderByDescending(n => n.CreatedDate)
            .Take(limit)
            .Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedDate = n.CreatedDate,
                NotificationType = n.NotificationType,
                ActionUrl = n.ActionUrl,
                TimeAgo = GetTimeAgo(n.CreatedDate)
            })
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
            .CountAsync();
    }

    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null) return false;

        notification.IsRead = true;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var item in unread)
        {
            item.IsRead = true;
        }

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<NotificationDto> CreateNotificationAsync(int? userId, string title, string message, string type, string actionUrl)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedDate = DateTime.Now
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return new NotificationDto
        {
            NotificationId = notification.NotificationId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedDate = notification.CreatedDate,
            NotificationType = notification.NotificationType,
            ActionUrl = notification.ActionUrl,
            TimeAgo = "vừa xong"
        };
    }

    private static string GetTimeAgo(DateTime date)
    {
        var timeSpan = DateTime.Now.Subtract(date);
        
        if (timeSpan <= TimeSpan.FromSeconds(60))
            return "vừa xong";
        if (timeSpan <= TimeSpan.FromMinutes(60))
            return timeSpan.Minutes > 1 ? string.Format("{0} phút trước", timeSpan.Minutes) : "1 phút trước";
        if (timeSpan <= TimeSpan.FromHours(24))
            return timeSpan.Hours > 1 ? string.Format("{0} giờ trước", timeSpan.Hours) : "1 giờ trước";
        if (timeSpan <= TimeSpan.FromDays(30))
            return timeSpan.Days > 1 ? string.Format("{0} ngày trước", timeSpan.Days) : "1 ngày trước";
        if (timeSpan <= TimeSpan.FromDays(365))
            return timeSpan.Days > 30 ? string.Format("{0} tháng trước", timeSpan.Days / 30) : "1 tháng trước";
        
        return timeSpan.Days > 365 ? string.Format("{0} năm trước", timeSpan.Days / 365) : "1 năm trước";
    }
}
