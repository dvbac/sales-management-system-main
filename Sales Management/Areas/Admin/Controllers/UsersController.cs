using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IHubContext<SystemHub> _hubContext;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, IHubContext<SystemHub> hubContext, ILogger<UsersController> logger)
        {
            _userService = userService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: Admin/Users (Lấy danh sách người dùng)
        public async Task<IActionResult> Index(string search, string role, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.CurrentFilter = search;
            ViewBag.CurrentRole = role;

            var users = await _userService.GetAdminUsersAsync(search, role, sortOrder);
            return View(users);
        }

        // GET: Admin/Users/Create (Form tạo mới người dùng)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create (Xử lý tạo người dùng mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Role,IsActive")] User user, string Password)
        {
            ModelState.Remove("Username");
            ModelState.Remove("PasswordHash");

            if (ModelState.IsValid)
            {
                var result = await _userService.CreateUserAsync(user, Password);
                if (result)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "ReloadData");
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("Email", "Email already exists or internal error.");
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/5 (Form chỉnh sửa người dùng)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Users/Edit/5 (Cập nhật người dùng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Email,FullName,PhoneNumber,Role,IsActive")] User user, string? NewPassword)
        {
            if (id != user.UserId) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _userService.UpdateUserAsync(user, NewPassword);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Internal error updating user.");
            }
            return View(user);
        }

        // GET: Admin/Users/Delete/5 (Xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userService.GetUserByIdAsync(id.Value);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Admin/Users/Delete/5 (Xử lý xóa mềm người dùng)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _userService.SoftDeleteUserAsync(id);
            if (result)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveUpdate", "ReloadData");
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
