using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Web.Models;
using SalesManagement.BLL.Interfaces;
using SalesManagement.Web.Models;
using Microsoft.Extensions.Logging;

namespace SalesManagement.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICategoryService categoryService)
        {
            _logger = logger;
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? categoryId, int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            if (searchString != null) pageNumber = 1;
            else searchString = currentFilter;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryId;

            var products = await _productService.GetPagedProductsAsync(pageNumber, pageSize, searchString, sortOrder, categoryId);
            var totalCount = await _productService.GetTotalProductCountAsync(searchString, categoryId);
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
                    CategoryName = p.Category?.Name,
                    SellingPrice = p.SellingPrice,
                    StockQuantity = p.StockQuantity,
                    Status = p.Status,
                    PrimaryImageUrl = p.ProductImages.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl 
                                    ?? p.ProductImages.FirstOrDefault()?.ImageUrl
                }).ToList(),
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                SearchString = searchString,
                SortOrder = sortOrder,
                CategoryId = categoryId,
                Categories = categories.Select(c => new CategoryViewModel
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                }).ToList()
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductList", viewModel);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _productService.GetProductByIdAsync(id.Value);

            if (product == null || product.Status == "Deleted")
            {
                return NotFound();
            }

            var relatedProducts = await _productService.GetRelatedProductsAsync(product.CategoryId, product.ProductId, 6);
            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
