using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string Code { get; set; } = null!;

    public string DiscountType { get; set; } = null!;

    public decimal Value { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? MinOrderValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<OrderPromotion> OrderPromotions { get; set; } = new List<OrderPromotion>();
}
