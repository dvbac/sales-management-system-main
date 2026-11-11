using System.Collections.Generic;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.DTOs
{
    public class DashboardDTO
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int NewUsers { get; set; }
        public IEnumerable<Order>? RecentOrders { get; set; }
        public List<int>? RevenueData { get; set; }
        public List<string>? CategoryLabels { get; set; }
        public List<int>? CategoryData { get; set; }
    }
}
