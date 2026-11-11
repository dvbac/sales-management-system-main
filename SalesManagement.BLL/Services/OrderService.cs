using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System.Data;

namespace SalesManagement.BLL.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _context;
        private readonly IPromotionService _promotionService;

        public OrderService(IOrderRepository orderRepository, AppDbContext context, IPromotionService promotionService)
        {
            _orderRepository = orderRepository;
            _context = context;
            _promotionService = promotionService;
        }

        public async Task<IEnumerable<Order>> GetOrdersOverviewAsync(string? searchString, string? statusFilter)
        {
            return await _orderRepository.GetOrdersWithCustomerAsync(searchString, statusFilter);
        }

        public async Task<Order?> GetOrderDetailsAsync(int id)
        {
            return await _orderRepository.GetOrderDetailsAsync(id);
        }

        public async Task<IEnumerable<Order>> GetMyOrdersAsync(int userId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (customer == null) return new List<Order>();

            return await _context.Orders
                .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            if (status == "Cancelled") return await CancelOrderAsync(id, false);
            if (status != "Draft" && status != "Confirmed" && status != "Paid" && status != "Completed")
                return false;

            var order = await _context.Orders.Include(o => o.Invoices).FirstOrDefaultAsync(o => o.OrderId == id);
            if (order == null || order.Status == "Cancelled" || order.PaymentStatus == "Refunded") return false;
            // Historical payment methods require manual reconciliation, not a new cash payment.
            if (order.PaymentMethod != "Cash" && order.PaymentMethod != "BankTransfer") return false;
            if (order.PaymentStatus == "Paid" && status != "Paid" && status != "Completed") return false;

            order.Status = status;
            if (status == "Paid" || status == "Completed")
            {
                order.PaymentStatus = "Paid";
                foreach (var invoice in order.Invoices) invoice.Status = "Paid";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Order?> CreateOrderAsync(int customerId, List<int> productIds, List<int> quantities)
        {
            if (productIds == null || quantities == null || productIds.Count == 0 ||
                productIds.Count != quantities.Count || quantities.Any(q => q <= 0)) return null;

            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                if (!await _context.Customers.AnyAsync(c => c.CustomerId == customerId)) return null;

                var order = new Order
                {
                    CustomerId = customerId,
                    OrderDate = DateTime.Now,
                    Status = "Confirmed",
                    PaymentStatus = "Unpaid",
                    PaymentMethod = "Cash",
                    CreatedBy = 1
                };

                decimal subTotal = 0;
                for (int i = 0; i < productIds.Count; i++)
                {
                    var product = await _context.Products.FindAsync(productIds[i]);
                    if (product == null || product.Status != "Active" || product.SellingPrice <= 0 ||
                        quantities[i] > (product.StockQuantity ?? 0)) return null;

                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = product.ProductId,
                        Quantity = quantities[i],
                        UnitPrice = product.SellingPrice,
                        Total = quantities[i] * product.SellingPrice
                    });
                    subTotal += quantities[i] * product.SellingPrice;
                    product.StockQuantity -= quantities[i];
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = product.ProductId,
                        Quantity = -quantities[i],
                        CreatedDate = DateTime.Now,
                        Type = "Export_Order",
                        CreatedBy = 1
                    });
                }

                order.SubTotal = subTotal;
                order.TaxAmount = subTotal * 0.1m;
                order.TotalAmount = subTotal + order.TaxAmount;
                order.Invoices.Add(new Invoice
                {
                    InvoiceDate = DateTime.Now,
                    Amount = order.TotalAmount ?? 0,
                    Status = "Issued"
                });
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return order;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return null;
            }
        }

        public Task<(bool Success, string Message, int OrderId)> CheckoutAsync(int userId, int productId, int quantity)
        {
            return CheckoutCartAsync(userId, new List<(int, int)> { (productId, quantity) });
        }

        public async Task<(bool Success, string Message, int OrderId)> CheckoutCartAsync(
            int userId, List<(int productId, int quantity)> items, string? promoCode = null)
        {
            if (items == null || items.Count == 0) return (false, "Giỏ hàng trống!", 0);
            if (items.Any(i => i.quantity <= 0) || items.Select(i => i.productId).Distinct().Count() != items.Count)
                return (false, "Sản phẩm hoặc số lượng không hợp lệ!", 0);

            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
                if (customer == null)
                    return (false, "Bạn cần hoàn thiện hồ sơ khách hàng trước khi mua hàng!", 0);

                decimal subTotal = 0;
                var processedItems = new List<(Product product, int quantity)>();
                foreach (var item in items)
                {
                    var product = await _context.Products.FindAsync(item.productId);
                    if (product == null || product.Status != "Active" || product.SellingPrice <= 0)
                        return (false, "Sản phẩm không tồn tại hoặc không còn bán!", 0);
                    if ((product.StockQuantity ?? 0) < item.quantity)
                        return (false, $"Sản phẩm '{product.Name}' không đủ số lượng trong kho!", 0);

                    subTotal += product.SellingPrice * item.quantity;
                    processedItems.Add((product, item.quantity));
                }

                decimal discount = 0;
                if (!string.IsNullOrWhiteSpace(promoCode))
                {
                    var promoResult = await _promotionService.ValidatePromotionAsync(promoCode, subTotal);
                    if (!promoResult.Success) return (false, promoResult.Message, 0);
                    discount = Math.Clamp(promoResult.DiscountAmount, 0, subTotal);
                }

                var order = new Order
                {
                    CustomerId = customer.CustomerId,
                    OrderDate = DateTime.Now,
                    SubTotal = subTotal,
                    TaxAmount = 0,
                    DiscountAmount = discount,
                    TotalAmount = subTotal - discount,
                    Status = "Confirmed",
                    PaymentStatus = "Unpaid",
                    PaymentMethod = "Cash",
                    CreatedBy = userId
                };
                order.Invoices.Add(new Invoice
                {
                    Amount = order.TotalAmount.Value,
                    InvoiceDate = DateTime.Now,
                    Status = "Issued"
                });

                foreach (var item in processedItems)
                {
                    item.product.StockQuantity -= item.quantity;
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.product.ProductId,
                        Quantity = item.quantity,
                        UnitPrice = item.product.SellingPrice,
                        Total = item.product.SellingPrice * item.quantity
                    });
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = item.product.ProductId,
                        Quantity = -item.quantity,
                        CreatedDate = DateTime.Now,
                        Type = "Export_Order",
                        CreatedBy = userId
                    });
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (true, "Đặt hàng thành công! Vui lòng thanh toán tiền mặt khi nhận hàng.", order.OrderId);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Không thể đặt hàng. Vui lòng thử lại.", 0);
            }
        }

        public Task<bool> RefundOrderAsync(int id)
        {
            return CancelOrderAsync(id, true);
        }

        private async Task<bool> CancelOrderAsync(int id, bool confirmRefund)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
                    .Include(o => o.Invoices)
                    .FirstOrDefaultAsync(o => o.OrderId == id);
                if (order == null || order.Status == "Cancelled" || order.PaymentStatus == "Refunded") return false;
                if (order.PaymentMethod != "Cash" && order.PaymentMethod != "BankTransfer") return false;
                var isPaid = order.PaymentStatus == "Paid";
                // Recording a refund requires the operator to have returned the money outside the app.
                if (confirmRefund != isPaid) return false;

                foreach (var item in order.OrderDetails)
                {
                    item.Product.StockQuantity += item.Quantity;
                    _context.InventoryTransactions.Add(new InventoryTransaction
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Type = "Import",
                        Note = $"Hoàn kho từ đơn hàng #ORD-{order.OrderId:D5}",
                        CreatedDate = DateTime.Now,
                        CreatedBy = order.CreatedBy
                    });
                }

                order.Status = "Cancelled";
                if (isPaid) order.PaymentStatus = "Refunded";
                foreach (var invoice in order.Invoices) invoice.Status = "Cancelled";
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
