using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Web.Areas.Admin.Models;
using SalesManagement.BLL.Interfaces;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: /Admin/Reports/Index
        public async Task<IActionResult> Index(
            string timeframe = "month",
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var dto = await _reportService.GetRevenueReportAsync(timeframe, startDate, endDate);

            var revenueGrowth = dto.PrevRevenue > 0
                ? Math.Round(((dto.TotalRevenue - dto.PrevRevenue) / dto.PrevRevenue) * 100, 1)
                : (dto.TotalRevenue > 0 ? 100m : 0m);

            var ordersGrowth = dto.PrevOrdersCount > 0
                ? Math.Round((decimal)(dto.CurrentOrdersCount - dto.PrevOrdersCount) / dto.PrevOrdersCount * 100, 1)
                : (dto.CurrentOrdersCount > 0 ? 100m : 0m);

            var model = new RevenueReportViewModel
            {
                Timeframe        = timeframe,
                StartDate        = dto.Start,
                EndDate          = dto.End,
                TotalRevenue     = dto.TotalRevenue,
                TotalOrders      = dto.CurrentOrdersCount,
                AverageOrderValue = dto.CurrentOrdersCount > 0
                    ? dto.TotalRevenue / dto.CurrentOrdersCount : 0,
                CompletionRate   = dto.CurrentOrdersCount > 0
                    ? Math.Round((double)dto.CompletedCount / dto.CurrentOrdersCount * 100, 1) : 0,
                CancellationRate = dto.CurrentOrdersCount > 0
                    ? Math.Round((double)dto.CancelledCount / dto.CurrentOrdersCount * 100, 1) : 0,
                RevenueGrowth    = revenueGrowth,
                OrdersGrowth     = ordersGrowth,
                // Chart data comes pre-built from service
                ChartLabels      = dto.ChartData.Select(p => p.Label).ToList(),
                RevenueChartData = dto.ChartData.Select(p => p.Revenue).ToList(),
                OrdersChartData  = dto.ChartData.Select(p => p.Orders).ToList()
            };

            return View(model);
        }

        // GET: /Admin/Reports/Products
        public async Task<IActionResult> Products(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var dto = await _reportService.GetProductReportAsync(startDate, endDate);

            var model = new ProductReportViewModel
            {
                StartDate           = dto.Start,
                EndDate             = dto.End,
                TotalItemsInStock   = dto.TotalItemsInStock,
                TotalInventoryValue = dto.TotalInventoryValue,
                TopSellingProducts  = dto.TopSelling.Select(x => new TopProductViewModel
                {
                    ProductName      = x.ProductName,
                    ProductCode      = x.ProductCode,
                    UnitsSold        = x.UnitsSold,
                    RevenueGenerated = x.RevenueGenerated
                }).ToList(),
                LowStockProducts = dto.LowStock.Select(p => new ProductInventoryViewModel
                {
                    ProductId     = p.ProductId,
                    Name          = p.Name,
                    Code          = p.Code,
                    StockQuantity = p.StockQuantity,
                    Value         = p.Value
                }).ToList()
            };

            return View(model);
        }

        // GET: /Admin/Reports/Financials
        public async Task<IActionResult> Financials(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var dto = await _reportService.GetFinancialReportAsync(startDate, endDate);

            var model = new FinancialReportViewModel
            {
                StartDate    = dto.Start,
                EndDate      = dto.End,
                NetRevenue   = dto.PaidRevenue,
                InvoiceStats = new System.Collections.Generic.List<InvoiceStatViewModel>
                {
                    new() { Status = "Paid",     Count = dto.PaidCount,     TotalAmount = dto.PaidRevenue    },
                    new() { Status = "Pending",  Count = dto.PendingCount,  TotalAmount = dto.PendingRevenue },
                    new() { Status = "Refunded", Count = dto.RefundedCount, TotalAmount = dto.RefundedRevenue }
                }
            };

            return View(model);
        }

        // GET: /Admin/Reports/ExportRevenue
        public async Task<IActionResult> ExportRevenue(string timeframe = "month")
        {
            var dto = await _reportService.GetRevenueReportAsync(timeframe, null, null);
            var csv = new StringBuilder();
            csv.AppendLine("Ngay,DonHang,DoanhThu");
            foreach (var p in dto.ChartData)
                csv.AppendLine($"{p.Label},{p.Orders},{p.Revenue}");

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                $"RevenueReport_{DateTime.Now:yyyyMMdd}.csv");
        }

        // GET: /Admin/Reports/ExportProducts
        public async Task<IActionResult> ExportProducts()
        {
            var dto = await _reportService.GetProductReportAsync(null, null);
            var csv = new StringBuilder();
            csv.AppendLine("Id,Code,Name,Stock,Value");
            foreach (var p in dto.LowStock)
                csv.AppendLine($"{p.ProductId},{p.Code},{p.Name},{p.StockQuantity},{p.Value}");

            return File(
                Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                $"InventoryReport_{DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
