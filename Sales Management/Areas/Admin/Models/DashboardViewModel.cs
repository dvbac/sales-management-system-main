using SalesManagement.DAL.Entities;

namespace SalesManagement.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int NewUsers { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
        
        // Data for charts
        public List<int> RevenueData { get; set; } = new List<int>(); // 12 months
        public List<string> CategoryLabels { get; set; } = new List<string>();
        public List<int> CategoryData { get; set; } = new List<int>();
        
    }
}
