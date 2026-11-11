using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountsController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<DiscountsController> _logger;

        public DiscountsController(IPromotionService promotionService, ILogger<DiscountsController> logger)
        {
            _promotionService = promotionService;
            _logger = logger;
        }

        // GET: Admin/Discounts (Danh sách mã giảm giá)
        public async Task<IActionResult> Index(string statusFilter, string searchString)
        {
            var promotions = await _promotionService.GetAllPromotionsAsync(statusFilter, searchString);
            ViewData["StatusFilter"] = statusFilter;
            ViewData["CurrentFilter"] = searchString;
            return View(promotions);
        }

        // GET: Admin/Discounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Discounts/Create (Tạo mã giảm giá mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Promotion promotion)
        {
            if (ModelState.IsValid)
            {
                var result = await _promotionService.AddPromotionAsync(promotion);
                if (result)
                {
                    _logger.LogInformation($"Created new voucher {promotion.Code}.");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("EndDate", "End Date must be greater than Start Date.");
            }
            return View(promotion);
        }

        // GET: Admin/Discounts/Edit/5 (Chỉnh sửa mã giảm giá)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var promotion = await _promotionService.GetPromotionByIdAsync(id.Value);
            if (promotion == null) return NotFound();

            // Kiểm tra trạng thái hết hạn thông qua View Model logic
            if (promotion.Status == "Expired" || (promotion.EndDate.HasValue && promotion.EndDate < DateTime.Now))
            {
                ViewBag.IsExpired = true;
            }

            return View(promotion);
        }

        // POST: Admin/Discounts/Edit/5 (Lưu thay đổi mã giảm giá)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Promotion promotion)
        {
            if (id != promotion.PromotionId) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _promotionService.UpdatePromotionAsync(promotion);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Cannot update voucher. Check dates.");
            }
            return View(promotion);
        }

        // GET: Admin/Discounts/Delete/5 (Xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var promotion = await _promotionService.GetPromotionByIdAsync(id.Value);
            if (promotion == null) return NotFound();

            return View(promotion);
        }

        // POST: Admin/Discounts/Delete/5 (Xóa mã giảm giá)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _promotionService.DeletePromotionAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
