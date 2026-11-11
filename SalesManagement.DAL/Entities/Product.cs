using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesManagement.DAL.Entities
{
    public partial class Product
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        public string Code { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public decimal? ImportPrice { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán phải lớn hơn 0")]
        public decimal SellingPrice { get; set; }

        public decimal? Vatrate { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng sản phẩm không được âm")]
        public int? StockQuantity { get; set; }

        public string? Status { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }


        // Navigation properties
        public virtual Category? Category { get; set; }
        public virtual User? CreatedByNavigation { get; set; }
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}
