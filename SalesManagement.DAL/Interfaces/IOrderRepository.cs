using SalesManagement.DAL.Entities;

namespace SalesManagement.DAL.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersWithCustomerAsync(string? searchString, string? statusFilter);
        Task<Order?> GetOrderDetailsAsync(int id);
    }
}
