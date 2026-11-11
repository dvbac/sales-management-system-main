using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        // GET: Admin/Settings
        public async Task<IActionResult> Index()
        {
            var settings = (await _settingService.GetAllSettingsAsync())
                           .ToDictionary(s => s.SettingKey, s => s.SettingValue);

            // Populate ViewData with safe defaults — prevents NullReferenceException
            // when SystemSettings table is empty or key is missing
            ViewData["StoreName"]       = settings.GetValueOrDefault("StoreName",       "Sales Management");
            ViewData["StoreEmail"]      = settings.GetValueOrDefault("StoreEmail",      "");
            ViewData["StoreAddress"]    = settings.GetValueOrDefault("StoreAddress",    "");
            ViewData["MaintenanceMode"] = settings.GetValueOrDefault("MaintenanceMode", "false") == "true";

            return View();
        }

        // POST: Admin/Settings/UpdateGeneral
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGeneral(
            string StoreName, string StoreEmail, string StoreAddress,
            bool MaintenanceMode)
        {
            var updates = new[]
            {
                ("StoreName",       StoreName       ?? ""),
                ("StoreEmail",      StoreEmail      ?? ""),
                ("StoreAddress",    StoreAddress    ?? ""),
                ("Currency",        "VND"),
                ("MaintenanceMode", MaintenanceMode ? "true" : "false")
            };

            foreach (var (key, value) in updates)
            {
                var existing = await _settingService.GetSettingByKeyAsync(key);
                if (existing != null)
                {
                    existing.SettingValue = value;
                    await _settingService.UpdateSettingAsync(existing);
                }
                else
                {
                    await _settingService.AddSettingAsync(new SystemSetting
                    {
                        SettingKey   = key,
                        SettingValue = value,
                        GroupName    = "General"
                    });
                }
            }

            TempData["SuccessMessage"] = "Đã lưu cài đặt thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Settings/UpdatePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePassword(
            string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction(nameof(Index));
            }

            // Password change logic can be wired to IAuthService here
            TempData["SuccessMessage"] = "Tính năng đổi mật khẩu đang được phát triển.";
            return RedirectToAction(nameof(Index));
        }
    }
}
