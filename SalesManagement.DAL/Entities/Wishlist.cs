using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.DAL.Entities;

public class Wishlist
{
    [Key]
    public int WishlistId { get; set; }

    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; }

    public int ProductId { get; set; }

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
