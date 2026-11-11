namespace SalesManagement.BLL.DTOs
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
