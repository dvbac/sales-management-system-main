using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using SalesManagement.DAL.Entities;
using SalesManagement.BLL.Interfaces;
using SalesManagement.Web.Filters;

namespace SalesManagement.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public AdminProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Admin/Products (Danh sách sản phẩm, quản lý tìm kiếm, sắp xếp và phân trang)
        [PreventDuplicateSubmission(300)]
        public async Task<IActionResult> Index(string searchString, int? page, string sortOrder)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["PriceSortParm"] = sortOrder == "Price" ? "price_desc" : "Price";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            int pageSize = 10;
            int pageNumber = page ?? 1;

            var products = await _productService.GetPagedProductsAsync(pageNumber, pageSize, searchString, sortOrder);
            var count = await _productService.GetTotalProductCountAsync(searchString);

            ViewBag.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.SearchString = searchString;
            ViewBag.SortOrder = sortOrder;
            
            return View(products);
        }

        // GET: Admin/Products/Details/5 (Xem chi tiết sản phẩm)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductDetailsAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Admin/Products/Create (Form tạo sản phẩm mới)
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDuplicateSubmission(1000)]
        public async Task<IActionResult> Create([Bind("Code,Name,Description,CategoryId,ImportPrice,SellingPrice,Vatrate,StockQuantity,Status")] Product product, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Xử lý Hình ảnh (Logic trong Web Layer là chấp nhận được để tránh Web-specific dependency trong BLL)
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    bool isFirst = true;
                    if (product.ProductImages == null) product.ProductImages = new List<ProductImage>();

                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                            
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            product.ProductImages.Add(new ProductImage
                            {
                                ImageUrl = "/images/" + fileName,
                                IsPrimary = isFirst, 
                                CreatedDate = DateTime.Now
                            });
                            isFirst = false; 
                        }
                    }
                }

                // Gán người tạo từ Claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    product.CreatedBy = userId;
                }

                var result = await _productService.AddProductAsync(product);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("Code", "Product code already exists.");
            }
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Edit/5 (Form chỉnh sửa sản phẩm)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Products/Edit/5 (Lưu thay đổi sản phẩm)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [PreventDuplicateSubmission(1000)]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Code,Name,Description,CategoryId,ImportPrice,SellingPrice,Vatrate,StockQuantity,Status,CreatedBy,CreatedDate")] Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                // Xử lý ảnh mới nếu có
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    if (product.ProductImages == null) product.ProductImages = new List<ProductImage>();
                    
                    product.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = "/images/" + fileName,
                        IsPrimary = true,
                        CreatedDate = DateTime.Now
                    });
                }

                var result = await _productService.UpdateProductAsync(product);
                if (result)
                {
                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                
                ModelState.AddModelError("Code", "Product code already exists.");
            }
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/5 (Xác nhận xóa)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductDetailsAsync(id.Value);
            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Admin/Products/Delete/5 (Xử lý xóa mềm)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [PreventDuplicateSubmission(1000)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteProductAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Product deleted successfully (Soft Delete).";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not delete product.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
