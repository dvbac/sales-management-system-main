using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class InventoryTransaction
{
    public int TransId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public string Type { get; set; } = null!;

    public string? Note { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual User? CreatedByNavigation { get; set; }

    public virtual Product Product { get; set; } = null!;
}
