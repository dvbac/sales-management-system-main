using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.DAL.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersWithCustomerAsync(string? searchString, string? statusFilter)
        {
            var query = _dbSet
                .Include(o => o.Customer)
                .Include(o => o.CreatedByNavigation)
                .OrderByDescending(o => o.OrderDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderId.ToString().Contains(searchString) || 
                                         o.Customer.FullName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            return await query.ToListAsync();
        }

        public async Task<Order?> GetOrderDetailsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.Customer)
                .Include(o => o.CreatedByNavigation)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);
        }
    }
}
