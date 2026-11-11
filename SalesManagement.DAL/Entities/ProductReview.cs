using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.DAL.Entities;

public class ProductReview
{
    [Key]
    public int ReviewId { get; set; }

    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }

    public int UserId { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string Comment { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
    
    public bool IsVerifiedPurchase { get; set; }
}
