using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.DAL.Entities;

public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    public int? UserId { get; set; } // Null if it's a system broadcast

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    public string Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [StringLength(50)]
    public string NotificationType { get; set; }

    [StringLength(255)]
    public string ActionUrl { get; set; }
}
