using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Web.Areas.Admin.Models;
using System.Threading.Tasks;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using System.Linq;

using SalesManagement.Web.Filters;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [PreventDuplicateSubmission(500)]
        public async Task<IActionResult> Index()
        {
            var stats = await _dashboardService.GetDashboardStatsAsync();

            var viewModel = new DashboardViewModel
            {
                TotalRevenue = stats.TotalRevenue,
                TotalOrders = stats.TotalOrders,
                PendingOrders = stats.PendingOrders,
                NewUsers = stats.NewUsers,
                RecentOrders = stats.RecentOrders?.ToList() ?? new List<Order>(),
                RevenueData = stats.RevenueData ?? new List<int>(),
                CategoryLabels = stats.CategoryLabels ?? new List<string>(),
                CategoryData = stats.CategoryData ?? new List<int>()
            };

            return View(viewModel);
        }
    }
}
