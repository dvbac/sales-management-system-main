using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetOrdersOverviewAsync(string? searchString, string? statusFilter);
        Task<Order?> GetOrderDetailsAsync(int id);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<Order?> CreateOrderAsync(int customerId, List<int> productIds, List<int> quantities);
        Task<(bool Success, string Message, int OrderId)> CheckoutAsync(int userId, int productId, int quantity);
        Task<(bool Success, string Message, int OrderId)> CheckoutCartAsync(int userId, List<(int productId, int quantity)> items, string? promoCode = null);
        Task<IEnumerable<Order>> GetMyOrdersAsync(int userId);
        Task<bool> RefundOrderAsync(int id);
    }
}
