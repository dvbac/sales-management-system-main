using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers;

public class ProductReviewsController : Controller
{
    private readonly IProductReviewService _productReviewService;

    public ProductReviewsController(IProductReviewService productReviewService)
    {
        _productReviewService = productReviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReviews(int productId)
    {
        var reviews = await _productReviewService.GetProductReviewsAsync(productId);
        // Trả về JSON để render client-side hoặc trả về PartialView tùy kiến trúc
        return Json(new { success = true, data = reviews });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview(int productId, int rating, string comment)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var success = await _productReviewService.AddReviewAsync(userId, productId, rating, comment);
            return Json(new { success = success, message = success ? "Đã gửi đánh giá thành công!" : "Không thể gửi đánh giá." });
        }
        return Json(new { success = false, message = "Vui lòng đăng nhập" });
    }
}
