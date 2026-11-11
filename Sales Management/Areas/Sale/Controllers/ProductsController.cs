using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;

namespace SalesManagement.Web.Areas.Sale.Controllers
{
    [Area("Sale")]
    [Authorize(Roles = "Sales, Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(IProductService productService, ICategoryService categoryService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Sale/Products
        public async Task<IActionResult> Index(int? categoryId, string? searchString)
        {
            var products = await _productService.GetPagedProductsAsync(1, 1000, searchString, null, categoryId);
            return View(products);
        }

        // GET: Sale/Products/Create
        public async Task<IActionResult> Create(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.CategoryId = categories
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToList();

            return View();
        }

        // POST: Sale/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var result = await _productService.AddProductAsync(product);
                if (result)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        await _productService.AddProductImageAsync(product.ProductId, "/images/products/" + fileName, true);
                    }
                    return RedirectToAction("Index", "Home", new { area = "Sale" });
                }
                ModelState.AddModelError("Code", "Mã sản phẩm đã tồn tại hoặc có lỗi xảy ra.");
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.CategoryId = categories.Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name }).ToList();
            return View(product);
        }

        // GET: Sale/Products/Edit/5
        public async Task<IActionResult> Edit(int? id, string? returnUrl)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl;
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", product.CategoryId);

            return View(product);
        }

        // POST: Sale/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageFile)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                var result = await _productService.UpdateProductAsync(product);
                if (result)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // 1. Get current images to delete physical file
                        var currentProduct = await _productService.GetProductByIdAsync(id);
                        if (currentProduct != null)
                        {
                            foreach (var oldImage in currentProduct.ProductImages)
                            {
                                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldImage.ImageUrl.TrimStart('/'));
                                if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // 2. Save new file
                        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
                        if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                        var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // 3. Update database: Clear and set new primary image
                        await _productService.RemoveProductImagesAsync(id);
                        await _productService.AddProductImageAsync(id, "/images/products/" + fileName, true);
                    }
                    return RedirectToAction("Index", "Home", new { area = "Sale" });
                }
                ModelState.AddModelError("Code", "Sản phẩm không tồn tại hoặc mã bị trùng.");
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Sale/Products/Delete/5
        public async Task<IActionResult> Delete(int? id, string? returnUrl)
        {
            if (id == null) return NotFound();
            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();
            ViewBag.ReturnUrl = returnUrl;
            return View(product);
        }

        // POST: Sale/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction("Index", "Home", new { area = "Sale" });
        }

        // GET: Sale/Products/Details/5
        public async Task<IActionResult> Details(int? id, string? returnUrl)
        {
            if (id == null) return NotFound();
            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null || product.Status == "Deleted") return NotFound();

            ViewBag.ReturnUrl = returnUrl ?? Url.Action("Index", "Home", new { area = "Sale" });
            return View(product);
        }
    }
}
