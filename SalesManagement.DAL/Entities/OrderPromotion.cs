using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class OrderPromotion
{
    public int OrderPromotionId { get; set; }

    public int OrderId { get; set; }

    public int PromotionId { get; set; }

    public decimal AppliedValue { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Promotion Promotion { get; set; } = null!;
}
