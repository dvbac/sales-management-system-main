using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesManagement.DAL.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Display(Name = "Hình ảnh")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Trạng thái")]
    public string? Status { get; set; }

    public bool IsDeleted { get; set; } = false;

    [Display(Name = "Thứ tự hiển thị")]
    public int? DisplayOrder { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? ParentId { get; set; }

    public virtual ICollection<Category> InverseParent { get; set; } = new List<Category>();

    public virtual Category? Parent { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
