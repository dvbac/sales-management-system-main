using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class Customer
{
    public int CustomerId { get; set; }

    public int? UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? Type { get; set; }

    public string? CustomerLevel { get; set; }

    public string? Note { get; set; }

    public DateTime? CreatedDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual User? User { get; set; }

}
