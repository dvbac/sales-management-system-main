using System;

namespace SalesManagement.BLL.DTOs;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedDate { get; set; }
    public string NotificationType { get; set; }
    public string ActionUrl { get; set; }
    public string TimeAgo { get; set; }
}
