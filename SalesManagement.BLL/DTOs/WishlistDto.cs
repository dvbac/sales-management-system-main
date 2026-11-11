namespace SalesManagement.BLL.DTOs;

public class WishlistDto
{
    public int WishlistId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
}
