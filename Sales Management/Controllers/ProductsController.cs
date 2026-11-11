using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.Web.Models;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        // GET: Products
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber, int? categoryId)
        {
            // Normalize categoryId - if 0 or less, treat as "All"
            if (categoryId.HasValue && categoryId.Value <= 0) categoryId = null;

            int pageSize = 12;
            int page = pageNumber ?? 1;

            var products = await _productService.GetPagedProductsAsync(page, pageSize, searchString, sortOrder, categoryId);
            int totalProducts = await _productService.GetTotalProductCountAsync(searchString, categoryId);
            int totalPages = (int)Math.Max(1, Math.Ceiling(totalProducts / (double)pageSize));

            var categories = await _categoryService.GetAllCategoriesAsync();

            var viewModel = new HomeProductViewModel
            {
                Products = products.Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? "General",
                    SellingPrice = p.SellingPrice,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    PrimaryImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl 
                                    ?? p.ProductImages.FirstOrDefault()?.ImageUrl
                                    ?? "/images/no-image.png"
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                SearchString = searchString,
                SortOrder = sortOrder,
                CategoryId = categoryId,
                Categories = categories.Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    DisplayOrder = c.DisplayOrder
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Products/Details/5
        // Redirect tới trang chi tiết đầy đủ của Home
        public IActionResult Details(int? id)
        {
            if (id == null) return NotFound();
            return RedirectToAction("Details", "Home", new { id });
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View(new ProductViewModel());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    SellingPrice = model.SellingPrice,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId > 0 ? model.CategoryId : 1
                };

                var result = await _productService.AddProductAsync(product);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("Code", "Sản phẩm với mã này đã tồn tại.");
            }
            return View(model);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                CategoryId = product.CategoryId,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity,
                Status = product.Status
            };

            return View(viewModel);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductId = model.ProductId,
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    SellingPrice = model.SellingPrice,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId
                };

                var result = await _productService.UpdateProductAsync(product);
                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Không thể cập nhật sản phẩm. Vui lòng thử lại.");
            }
            return View(model);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);
            if (product == null) return NotFound();

            var viewModel = new ProductViewModel
            {
                ProductId = product.ProductId,
                Code = product.Code,
                Name = product.Name,
                CategoryName = product.Category?.Name,
                SellingPrice = product.SellingPrice,
                StockQuantity = product.StockQuantity
            };

            return View(viewModel);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productService.DeleteProductAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
