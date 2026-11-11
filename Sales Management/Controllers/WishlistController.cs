using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    public async Task<IActionResult> Index()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var wishlist = await _wishlistService.GetUserWishlistAsync(userId);
            return View(wishlist);
        }
        return RedirectToAction("Login", "Account");
    }

    [HttpPost]
    public async Task<IActionResult> Add(int productId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var success = await _wishlistService.AddToWishlistAsync(userId, productId);
            return Json(new { success = success, message = success ? "Đã thêm vào danh sách yêu thích" : "Sản phẩm đã có trong danh sách hoặc có lỗi xảy ra" });
        }
        return Json(new { success = false, message = "Vui lòng đăng nhập" });
    }

    [HttpPost]
    public async Task<IActionResult> Remove(int productId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            var success = await _wishlistService.RemoveFromWishlistAsync(userId, productId);
            return Json(new { success = success, message = success ? "Đã xóa khỏi danh sách yêu thích" : "Có lỗi xảy ra" });
        }
        return Json(new { success = false, message = "Vui lòng đăng nhập" });
    }
}
