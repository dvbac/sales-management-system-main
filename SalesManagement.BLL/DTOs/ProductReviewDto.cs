using System;

namespace SalesManagement.BLL.DTOs;

public class ProductReviewDto
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; }
    public string Avatar { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsVerifiedPurchase { get; set; }
}
