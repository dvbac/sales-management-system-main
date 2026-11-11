using System;
using System.Collections.Generic;

namespace SalesManagement.Web.Models
{
    public class CustomerProfileViewModel
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? Avatar { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CustomerLevel { get; set; }
        public List<OrderHistoryViewModel> Orders { get; set; } = new();
    }

    public class OrderHistoryViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public string? ProductNames { get; set; }
    }

}
