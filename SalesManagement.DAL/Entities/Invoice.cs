using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int OrderId { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public decimal Amount { get; set; }

    public string? Status { get; set; }

    public virtual Order Order { get; set; } = null!;

}
