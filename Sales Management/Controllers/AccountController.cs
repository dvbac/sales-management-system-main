using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Web.Models;
using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SalesManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace SalesManagement.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;

        public AccountController(AppDbContext context, IAuthService authService, ICartService cartService)
        {
            _context = context;
            _authService = authService;
            _cartService = cartService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.IsActive && !u.IsDeleted);

            if (user == null)
            {
                ModelState.AddModelError("", "Email không tồn tại hoặc không thể đổi mật khẩu.");
                return View(model);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedDate = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult GoogleLogin(string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(GoogleCallback), "Account", new { returnUrl }) ?? "/";
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet]
        public async Task<IActionResult> GoogleCallback(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdStr, out int userId))
                {
                    await MergeSessionCart(userId);
                }
            }

            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            if (User.IsInRole("Sales"))
                return RedirectToAction("Index", "Home", new { area = "Sale" });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.ValidateUser(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View(model);
            }

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe 
                        ? DateTimeOffset.UtcNow.AddDays(30) 
                        : DateTimeOffset.UtcNow.AddHours(10)
                });

            // Merge Session Cart
            await MergeSessionCart(user.UserId);

            // Redirect theo role
            if (user.Role == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            else if (user.Role == "Sales")
                return RedirectToAction("Index", "Home", new { area = "Sale" });
            else
                return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.RegisterUser(
                model.Username,
                model.Email,
                model.Password,
                model.FullName,
                model.PhoneNumber
            );

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại");
                return View(model);
            }

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout(string? reason = null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
             var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
             if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction(nameof(Login));

             var user = await _authService.GetUserProfileAsync(userId);
             if (user == null) return NotFound();

             var customer = user.Customer;

             var model = new CustomerProfileViewModel
             {
                 FullName = user.FullName,
                 Email = user.Email,
                 PhoneNumber = user.PhoneNumber,
                 Address = customer?.Address,
                 Avatar = user.Avatar,
                 CreatedDate = user.CreatedDate,
                 CustomerLevel = customer?.CustomerLevel,
                 Orders = customer?.Orders.OrderByDescending(o => o.OrderDate).Select(o => new OrderHistoryViewModel
                 {
                      OrderId = o.OrderId,
                      OrderDate = o.OrderDate ?? DateTime.Now,
                      TotalAmount = o.TotalAmount ?? 0,
                      Status = o.Status, ProductNames = string.Join(", ", o.OrderDetails.Select(d => d.Product?.Name))
                 }).ToList() ?? new()
             };

             return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string FullName, string Email, string PhoneNumber, string Address, IFormFile? AvatarFile)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId)) return RedirectToAction(nameof(Login));

            string? avatarUrl = null;
            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/avatars");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(AvatarFile.FileName);
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await AvatarFile.CopyToAsync(fileStream);
                }
                avatarUrl = "/img/avatars/" + fileName;
            }

            var success = await _authService.UpdateUserProfileAsync(userId, FullName, Email, PhoneNumber, Address, avatarUrl);
            if (success)
            {
                 TempData["Success"] = "Cập nhật thông tin thành công!";
            }
            else
            {
                 TempData["Error"] = "Cập nhật thất bại.";
            }

            return RedirectToAction(nameof(Profile));
        }

        private async Task MergeSessionCart(int userId)
        {
            try 
            {
                var json = HttpContext.Session.GetString("ShoppingCart");
                if (!string.IsNullOrEmpty(json))
                {
                    var sessionCart = System.Text.Json.JsonSerializer.Deserialize<List<CartItemViewModel>>(json);
                    if (sessionCart != null && sessionCart.Any())
                    {
                        var dtos = sessionCart.Select(i => new SalesManagement.BLL.DTOs.CartItemDTO
                        {
                            ProductId = i.ProductId,
                            Quantity = i.Quantity,
                            Name = i.Name,
                            ImageUrl = i.ImageUrl,
                            Price = i.Price
                        }).ToList();
                        await _cartService.MergeCartAsync(userId, dtos);
                        HttpContext.Session.Remove("ShoppingCart");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error if needed, but don't break login flow
                Console.WriteLine($"Error merging cart: {ex.Message}");
            }
        }
    }
}
