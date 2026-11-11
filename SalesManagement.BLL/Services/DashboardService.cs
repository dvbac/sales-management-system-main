using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SalesManagement.BLL.Interfaces;
using SalesManagement.BLL.DTOs;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;

namespace SalesManagement.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string DashboardCacheKey = "AdminDashboardStats";
        private static readonly System.Threading.SemaphoreSlim _lock = new System.Threading.SemaphoreSlim(1, 1);

        public DashboardService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<DashboardDTO> GetDashboardStatsAsync()
        {
            if (_cache.TryGetValue(DashboardCacheKey, out DashboardDTO stats))
            {
                return stats;
            }

            await _lock.WaitAsync();
            try
            {
                if (!_cache.TryGetValue(DashboardCacheKey, out stats))
                {
                    stats = await FetchDashboardStatsFromDb();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5)) // Cache for 5 minutes
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));

                    _cache.Set(DashboardCacheKey, stats, cacheOptions);
                }
                return stats;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<DashboardDTO> FetchDashboardStatsFromDb()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // 1. Basic Stats (Use Select for optimization)
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed" || o.PaymentStatus == "Paid")
                .SumAsync(o => o.TotalAmount) ?? 0;

            var totalOrders = await _context.Orders.CountAsync();

            var pendingOrders = await _context.Orders
                .CountAsync(o => o.Status == "Pending" || o.Status == "Processing");

            var newUsers = await _context.Users
                .CountAsync(u => u.CreatedDate.Month == currentMonth && u.CreatedDate.Year == currentYear);

            // 2. Recent Orders (Select only required columns)
            var recentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new Order {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Customer = o.Customer != null ? new Customer { FullName = o.Customer.FullName } : null
                })
                .ToListAsync();

            // 3. Revenue over months
            var monthlyRevenues = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderDate.HasValue && 
                            o.OrderDate.Value.Year == currentYear && 
                            (o.Status == "Completed" || o.PaymentStatus == "Paid"))
                .GroupBy(o => o.OrderDate!.Value.Month)
                .Select(g => new { 
                    Month = g.Key, 
                    Total = g.Sum(o => o.TotalAmount) ?? 0 
                })
                .ToListAsync();

            var revenueData = Enumerable.Range(1, 12).Select(month => {
                var monthData = monthlyRevenues.FirstOrDefault(m => m.Month == month);
                return (int)(monthData?.Total ?? 0);
            }).ToList();

            // 4. Category statistics (Efficient grouping)
            var categoryStats = await _context.OrderDetails
                .AsNoTracking()
                .Where(od => od.Product != null && od.Product.Category != null)
                .GroupBy(od => od.Product!.Category!.Name)
                .Select(g => new { 
                    CategoryName = g.Key, 
                    Count = g.Sum(od => od.Quantity) 
                }) 
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var categoryLabels = categoryStats.Select(x => x.CategoryName ?? "Unknown").ToList();
            var categoryData = categoryStats.Select(x => x.Count).ToList();

            return new DashboardDTO
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                NewUsers = newUsers,
                RecentOrders = recentOrders,
                RevenueData = revenueData,
                CategoryLabels = categoryLabels,
                CategoryData = categoryData
            };
        }
    }
}
