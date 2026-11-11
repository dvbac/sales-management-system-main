using System.ComponentModel.DataAnnotations;

namespace SalesManagement.Web.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Danh mục là bắt buộc")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public decimal? ImportPrice { get; set; }

        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal SellingPrice { get; set; }

        public decimal? Vatrate { get; set; }


        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
        public int? StockQuantity { get; set; }

        public string? Status { get; set; }
        
        public string? PrimaryImageUrl { get; set; }
    }
}
