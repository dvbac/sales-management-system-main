using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.DAL.Entities;
using Microsoft.AspNetCore.Hosting;
using SalesManagement.BLL.Interfaces;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoriesController(ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Categories (Lấy danh sách danh mục, hỗ trợ tìm kiếm và lọc)
        public async Task<IActionResult> Index(string searchString, string statusFilter, string sortOrder)
        {
            var categories = await _categoryService.GetAdminCategoriesAsync(searchString, statusFilter, sortOrder);

            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["OrderSortParm"] = sortOrder == "order" ? "order_desc" : "order";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            return View(categories);
        }

        // GET: Admin/Categories/Details/5 (Xem chi tiết danh mục)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null || category.IsDeleted) return NotFound();

            return View(category);
        }

        // GET: Admin/Categories/Create (Hiển thị form tạo mới)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create (Xử lý tạo mới danh mục)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Status,DisplayOrder")] Category category, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/categories");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    category.ImageUrl = "/images/categories/" + uniqueFileName;
                }

                var result = await _categoryService.AddCategoryAsync(category);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        // GET: Admin/Categories/Edit/5 (Hiển thị form chỉnh sửa)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null || category.IsDeleted) return NotFound();
            return View(category);
        }

        // POST: Admin/Categories/Edit/5 (Xử lý cập nhật danh mục)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description,Status,DisplayOrder,ImageUrl,CreatedDate,IsDeleted")] Category category, IFormFile? imageFile)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/categories");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                    
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                    category.ImageUrl = "/images/categories/" + uniqueFileName;
                }

                var result = await _categoryService.UpdateCategoryAsync(category);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5 (Hiển thị xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);
            if (category == null || category.IsDeleted) return NotFound();

            return View(category);
        }

        // POST: Admin/Categories/Delete/5 (Xử lý xóa mềm danh mục)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return NotFound();
        }
    }
}
