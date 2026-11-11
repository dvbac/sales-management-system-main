using Microsoft.AspNetCore.Mvc;
using SalesManagement.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using SalesManagement.BLL.Interfaces;
using System.Security.Claims;

namespace SalesManagement.Web.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(int productId, int quantity = 1)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();

            // Logic checkout được xử lý transactional trong BLL
            // Bao gồm: trừ kho, tạo đơn hàng và hóa đơn chờ thanh toán tiền mặt
            var result = await _orderService.CheckoutAsync(int.Parse(userIdString), productId, quantity);

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return RedirectToAction(nameof(MyOrders));
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();

            var orders = await _orderService.GetMyOrdersAsync(int.Parse(userIdString));
            return View(orders);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Index(string searchString, string statusFilter)
        {
            var orders = await _orderService.GetOrdersOverviewAsync(searchString, statusFilter);
            ViewData["CurrentFilter"] = searchString;
            ViewData["StatusFilter"] = statusFilter;
            return View(orders);
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _orderService.GetOrderDetailsAsync(id.Value);
            if (order == null) return NotFound();

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (!success)
                TempData["Error"] = "Không thể cập nhật đơn. Đơn đã thu tiền cần ghi nhận hoàn tiền trực tiếp trước khi hủy; đơn dùng phương thức cũ cần đối soát thủ công.";

            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Refund(int id)
        {
            var success = await _orderService.RefundOrderAsync(id);
            if (success)
            {
                TempData["Success"] = "Đã ghi nhận hoàn tiền trực tiếp và hủy đơn hàng.";
            }
            else
            {
                TempData["Error"] = "Chỉ ghi nhận hoàn tiền cho đơn tiền mặt/chuyển khoản đã thanh toán và chưa bị hủy. Đơn dùng phương thức cũ cần đối soát thủ công.";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}
