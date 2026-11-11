using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using SalesManagement.Web.Models;
using System.Security.Claims;
using System.Text.Json;

namespace SalesManagement.Web.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "ShoppingCart";
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPromotionService _promotionService;
        private readonly ICartService _cartService;

        public CartController(IProductService productService, IOrderService orderService, IPromotionService promotionService, ICartService cartService)
        {
            _productService = productService;
            _orderService = orderService;
            _promotionService = promotionService;
            _cartService = cartService;
        }

        // ──────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────
        private async Task<List<CartItemViewModel>> GetCartItemsAsync()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    var items = await _cartService.GetCartByUserIdAsync(userId);
                    return items.Select(i => new CartItemViewModel
                    {
                        ProductId = i.ProductId,
                        Name = i.Name,
                        ImageUrl = i.ImageUrl,
                        Price = i.Price,
                        Quantity = i.Quantity
                    }).ToList();
                }
            }

            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemViewModel>()
                : JsonSerializer.Deserialize<List<CartItemViewModel>>(json)!;
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            // Only used for guests. Authenticated users save directly to DB via service.
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // ──────────────────────────────────────────────
        // GET: /Cart/Index
        // ──────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var cart = await GetCartItemsAsync();
            var promotions = await _promotionService.GetAllPromotionsAsync("Active", null);
            var now = DateTime.Now;
            ViewBag.ActivePromotions = promotions.Where(p => 
                (p.StartDate == null || p.StartDate <= now) && 
                (p.EndDate == null || p.EndDate >= now)).ToList();

            return View(cart);
        }

        // ──────────────────────────────────────────────
        // POST: /Cart/AddToCart  (AJAX)
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            if (product.Status != "Active")
                return Json(new { success = false, message = "Sản phẩm không còn bán." });

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                
                // Stock check before adding to DB
                var existing = await _cartService.GetCartItemAsync(userId, productId);
                if ((existing?.Quantity ?? 0) + quantity > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });

                await _cartService.AddToCartAsync(userId, productId, quantity);
                int count = await _cartService.GetCartCountAsync(userId);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = count });
            }

            // GUEST FLOW
            var cart = await GetCartItemsAsync();
            var guestExisting = cart.FirstOrDefault(c => c.ProductId == productId);

            if (guestExisting != null)
            {
                var newQty = guestExisting.Quantity + quantity;
                if (newQty > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });
                guestExisting.Quantity = newQty;
            }
            else
            {
                if (quantity > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });

                var primaryImg = product.ProductImages?.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl
                              ?? product.ProductImages?.FirstOrDefault()?.ImageUrl
                              ?? "/images/no-image.png";

                cart.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    ImageUrl = primaryImg,
                    Price = product.SellingPrice,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            int totalItems = cart.Sum(c => c.Quantity);
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = totalItems });
        }

        // ──────────────────────────────────────────────
        // POST: /Cart/Remove
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _cartService.RemoveFromCartAsync(userId, productId);
            }
            else
            {
                var cart = await GetCartItemsAsync();
                cart.RemoveAll(c => c.ProductId == productId);
                SaveCart(cart);
            }
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────
        // POST: /Cart/UpdateQuantity  (AJAX)
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
                return Json(new { success = false, message = "Số lượng không hợp lệ." });

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _cartService.UpdateQuantityAsync(userId, productId, quantity);
                
                var items = await _cartService.GetCartByUserIdAsync(userId);
                var item = items.FirstOrDefault(i => i.ProductId == productId);
                decimal cartTotal = items.Sum(i => i.Total);
                
                return Json(new { 
                    success = true, 
                    newItemTotal = (item?.Total ?? 0).ToString("N0"), 
                    cartTotal = cartTotal.ToString("N0") 
                });
            }

            var cart = await GetCartItemsAsync();
            var guestItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (guestItem == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ." });

            guestItem.Quantity = quantity;
            SaveCart(cart);

            decimal newTotal = guestItem.Price * guestItem.Quantity;
            decimal total = cart.Sum(c => c.Price * c.Quantity);
            return Json(new { success = true, newItemTotal = newTotal.ToString("N0"), cartTotal = total.ToString("N0") });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutAll(List<int> selectedProductIds, string? promoCode = null)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString == null) return Unauthorized();
            int userId = int.Parse(userIdString);

            var fullCart = await GetCartItemsAsync();
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            var selectedItems = fullCart.Where(c => selectedProductIds.Contains(c.ProductId)).ToList();
            if (!selectedItems.Any())
            {
                TempData["Error"] = "Sản phẩm chọn mua không còn trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            var itemsToCheckout = selectedItems.Select(c => (c.ProductId, c.Quantity)).ToList();
            var result = await _orderService.CheckoutCartAsync(userId, itemsToCheckout, promoCode);

            if (result.Success)
            {
                // Chỉ xóa những sản phẩm đã đặt hàng thành công khỏi giỏ hàng
                if (User.Identity?.IsAuthenticated ?? false)
                {
                    foreach (var id in selectedProductIds)
                    {
                        await _cartService.RemoveFromCartAsync(userId, id);
                    }
                }
                else
                {
                    var updatedCart = fullCart.Where(c => !selectedProductIds.Contains(c.ProductId)).ToList();
                    SaveCart(updatedCart);
                }

                TempData["Success"] = result.Message;
                return RedirectToAction("MyOrders", "Orders");
            }
            else
            {
                TempData["Error"] = result.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ──────────────────────────────────────────────
        // GET: /Cart/Count  (AJAX — đếm số item)
        // ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                int count = await _cartService.GetCartCountAsync(userId);
                return Json(new { count = count });
            }
            var cart = await GetCartItemsAsync();
            return Json(new { count = cart.Sum(c => c.Quantity) });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ValidatePromo(string code, List<int> selectedProductIds)
        {
            var cart = await GetCartItemsAsync();
            var selectedItems = cart.Where(c => selectedProductIds != null && selectedProductIds.Contains(c.ProductId)).ToList();
            
            decimal totalOrderAmount = selectedItems.Sum(c => c.Price * c.Quantity);
            
            var result = await _promotionService.ValidatePromotionAsync(code, totalOrderAmount);
            return Json(new { 
                success = result.Success, 
                message = result.Message, 
                discountAmount = result.DiscountAmount,
                discountAmountDisplay = result.DiscountAmount.ToString("N0")
            });
        }
    }
}
