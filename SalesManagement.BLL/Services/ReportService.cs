using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SalesManagement.BLL.Interfaces;
using SalesManagement.BLL.DTOs;
using SalesManagement.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ReportService> _logger;

        // TTL for report cache entries
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

        public ReportService(AppDbContext context, IMemoryCache cache, ILogger<ReportService> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        // ─── Revenue Report ────────────────────────────────────────────────────
        public async Task<RevenueReportDTO> GetRevenueReportAsync(
            string timeframe, DateTime? startDate, DateTime? endDate)
        {
            var range = GetDateRange(timeframe, startDate, endDate);
            var cacheKey = $"Report_Revenue_{range.Start:yyyyMMdd}_{range.End:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out RevenueReportDTO? cached) && cached != null)
                return cached;

            var result = await BuildRevenueReportAsync(range.Start, range.End, timeframe);
            _cache.Set(cacheKey, result, CacheTtl);
            return result;
        }

        private async Task<RevenueReportDTO> BuildRevenueReportAsync(
            DateTime start, DateTime end, string timeframe)
        {
            // ── 1. Current-period stats (all aggregated in DB) ────────────────
            var currentStats = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalOrders    = g.Count(),
                    TotalRevenue   = g.Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                                      .Sum(o => o.TotalAmount) ?? 0m,
                    CompletedCount = g.Count(o => o.Status == "Completed" || o.PaymentStatus == "Paid"),
                    CancelledCount = g.Count(o => o.Status == "Cancelled"  || o.Status == "Returned")
                })
                .FirstOrDefaultAsync();

            // ── 2. Previous-period stats ──────────────────────────────────────
            var duration  = end - start;
            var prevStart = start.Subtract(duration);
            var prevEnd   = start.AddSeconds(-1);

            var prevStats = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= prevStart && o.OrderDate <= prevEnd)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalOrders  = g.Count(),
                    TotalRevenue = g.Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                                    .Sum(o => o.TotalAmount) ?? 0m
                })
                .FirstOrDefaultAsync();

            // ── 3. Chart data — aggregated by day or month entirely in the DB ─
            List<ChartPoint> chartPoints;

            if (timeframe == "year")
            {
                // Group by month number
                var monthlyData = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                    .GroupBy(o => o.OrderDate!.Value.Month)
                    .Select(g => new
                    {
                        Month   = g.Key,
                        Revenue = g.Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                                   .Sum(o => o.TotalAmount) ?? 0m,
                        Orders  = g.Count()
                    })
                    .ToListAsync();

                chartPoints = Enumerable.Range(1, 12).Select(m =>
                {
                    var d = monthlyData.FirstOrDefault(x => x.Month == m);
                    return new ChartPoint { Label = $"T{m}", Revenue = d?.Revenue ?? 0, Orders = d?.Orders ?? 0 };
                }).ToList();
            }
            else
            {
                // Group by day — let DB do the heavy lifting
                var dailyData = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                    .GroupBy(o => o.OrderDate!.Value.Date)
                    .Select(g => new
                    {
                        Day     = g.Key,
                        Revenue = g.Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                                   .Sum(o => o.TotalAmount) ?? 0m,
                        Orders  = g.Count()
                    })
                    .OrderBy(x => x.Day)
                    .ToListAsync();

                // Build a complete date-series (days with no orders get zero)
                var lookup = dailyData.ToDictionary(x => x.Day.Date);
                chartPoints = new List<ChartPoint>();
                for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
                {
                    lookup.TryGetValue(day, out var d);
                    chartPoints.Add(new ChartPoint
                    {
                        Label   = day.ToString("dd/MM"),
                        Revenue = d?.Revenue ?? 0,
                        Orders  = d?.Orders  ?? 0
                    });
                }
            }

            return new RevenueReportDTO
            {
                Start              = start,
                End                = end,
                TotalRevenue       = currentStats?.TotalRevenue   ?? 0,
                CurrentOrdersCount = currentStats?.TotalOrders    ?? 0,
                CompletedCount     = currentStats?.CompletedCount ?? 0,
                CancelledCount     = currentStats?.CancelledCount ?? 0,
                PrevRevenue        = prevStats?.TotalRevenue   ?? 0,
                PrevOrdersCount    = prevStats?.TotalOrders    ?? 0,
                ChartData          = chartPoints
            };
        }

        // ─── Product Report ────────────────────────────────────────────────────
        public async Task<ProductReportDTO> GetProductReportAsync(
            DateTime? startDate, DateTime? endDate)
        {
            var end   = endDate   ?? DateTime.Now;
            var start = startDate ?? new DateTime(end.Year, end.Month, 1);
            var cacheKey = $"Report_Product_{start:yyyyMMdd}_{end:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out ProductReportDTO? cached) && cached != null)
                return cached;

            var result = await BuildProductReportAsync(start, end);
            _cache.Set(cacheKey, result, CacheTtl);
            return result;
        }

        private async Task<ProductReportDTO> BuildProductReportAsync(DateTime start, DateTime end)
        {
            // ── Top-selling products — full GROUP BY in DB ────────────────────
            var topSelling = await _context.OrderDetails
                .AsNoTracking()
                .Where(od =>
                    od.Order.OrderDate >= start &&
                    od.Order.OrderDate <= end &&
                    (od.Order.Status == "Completed" || od.Order.PaymentStatus == "Paid"))
                .GroupBy(od => new { od.ProductId, od.Product.Name, od.Product.Code })
                .Select(g => new TopProductDTO
                {
                    ProductId       = g.Key.ProductId,
                    ProductName     = g.Key.Name,
                    ProductCode     = g.Key.Code,
                    UnitsSold       = g.Sum(od => od.Quantity),
                    RevenueGenerated = g.Sum(od => od.Total) ?? 0m
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(10)
                .ToListAsync();

            // ── Low-stock products — only fetch what we need ──────────────────
            var lowStock = await _context.Products
                .AsNoTracking()
                .Where(p => (p.StockQuantity ?? 0) < 10 && p.Status != "Deleted")
                .OrderBy(p => p.StockQuantity)
                .Take(20)
                .Select(p => new LowStockProductDTO
                {
                    ProductId     = p.ProductId,
                    Name          = p.Name,
                    Code          = p.Code,
                    StockQuantity = p.StockQuantity ?? 0,
                    Value         = (p.StockQuantity ?? 0) * p.SellingPrice
                })
                .ToListAsync();

            // ── Inventory summary — aggregate in DB ───────────────────────────
            var inventorySummary = await _context.Products
                .AsNoTracking()
                .Where(p => p.Status != "Deleted")
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    TotalItems = g.Count(),
                    TotalValue = g.Sum(p => (p.StockQuantity ?? 0) * p.SellingPrice)
                })
                .FirstOrDefaultAsync();

            return new ProductReportDTO
            {
                Start               = start,
                End                 = end,
                TopSelling          = topSelling,
                LowStock            = lowStock,
                TotalItemsInStock   = inventorySummary?.TotalItems ?? 0,
                TotalInventoryValue = inventorySummary?.TotalValue  ?? 0m
            };
        }

        // ─── Financial Report ──────────────────────────────────────────────────
        public async Task<FinancialReportDTO> GetFinancialReportAsync(
            DateTime? startDate, DateTime? endDate)
        {
            var end   = endDate   ?? DateTime.Now;
            var start = startDate ?? new DateTime(end.Year, end.Month, 1);
            var cacheKey = $"Report_Financial_{start:yyyyMMdd}_{end:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out FinancialReportDTO? cached) && cached != null)
                return cached;

            var result = await BuildFinancialReportAsync(start, end);
            _cache.Set(cacheKey, result, CacheTtl);
            return result;
        }

        private async Task<FinancialReportDTO> BuildFinancialReportAsync(DateTime start, DateTime end)
        {
            // ── 1. Invoice stats — one GROUP-BY query in DB ───────────────────
            var invoiceStats = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    PaidRevenue    = g.Where(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled").Sum(o => o.TotalAmount) ?? 0m,
                    PaidCount      = g.Count(o => o.PaymentStatus == "Paid" && o.Status != "Cancelled"),
                    PendingRevenue = g.Where(o => o.PaymentStatus == "Unpaid" && o.Status != "Cancelled").Sum(o => o.TotalAmount) ?? 0m,
                    PendingCount   = g.Count(o => o.PaymentStatus == "Unpaid" && o.Status != "Cancelled"),
                    RefundRevenue  = g.Where(o => o.PaymentStatus == "Refunded").Sum(o => o.TotalAmount) ?? 0m,
                    RefundCount    = g.Count(o => o.PaymentStatus == "Refunded")
                })
                .FirstOrDefaultAsync();

            return new FinancialReportDTO
            {
                Start           = start,
                End             = end,
                PaidRevenue     = invoiceStats?.PaidRevenue    ?? 0m,
                PaidCount       = invoiceStats?.PaidCount      ?? 0,
                PendingRevenue  = invoiceStats?.PendingRevenue ?? 0m,
                PendingCount    = invoiceStats?.PendingCount   ?? 0,
                RefundedRevenue = invoiceStats?.RefundRevenue  ?? 0m,
                RefundedCount   = invoiceStats?.RefundCount    ?? 0
            };
        }

        // ─── Helpers ───────────────────────────────────────────────────────────
        private static (DateTime Start, DateTime End) GetDateRange(
            string timeframe, DateTime? start, DateTime? end)
        {
            if (start.HasValue && end.HasValue) return (start.Value, end.Value);
            var now = DateTime.Now;
            return timeframe switch
            {
                "week" => (now.AddDays(-7), now),
                "year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31)),
                "all"  => (new DateTime(2000, 1, 1), now),
                _      => (new DateTime(now.Year, now.Month, 1),
                           new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1))
            };
        }
    }
}
