using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<ProductImage> _imageRepository;
        private readonly ICategoryService _categoryService;

        public ProductService(IProductRepository productRepository, IRepository<ProductImage> imageRepository, ICategoryService categoryService)
        {
            _productRepository = productRepository;
            _imageRepository = imageRepository;
            _categoryService = categoryService;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<Product?> GetProductDetailsAsync(int id)
        {
            return await _productRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Product>> GetPagedProductsAsync(int pageNumber, int pageSize, string searchString, string sortOrder, int? categoryId = null)
        {
            var query = _productRepository.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                // Filter out inactive/deleted products
                .Where(p => p.Status == null || (p.Status != "Inactive" && p.Status != "Deleted"));

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var categoryIds = await GetCategoryWithDescendantsAsync(categoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string search = searchString.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                       (p.Description != null && p.Description.ToLower().Contains(search)) ||
                                       p.Code.ToLower().Contains(search));
            }

            query = sortOrder switch
            {
                "name_desc" => query.OrderByDescending(p => p.Name),
                "Price" => query.OrderBy(p => p.SellingPrice),
                "price_desc" => query.OrderByDescending(p => p.SellingPrice),
                "Date" => query.OrderBy(p => p.CreatedDate),
                "date_desc" => query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            return await query
                .Skip((Math.Max(1, pageNumber) - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalProductCountAsync(string searchString, int? categoryId = null)
        {
            var query = _productRepository.GetQueryable()
                .Where(p => p.Status == null || (p.Status != "Inactive" && p.Status != "Deleted"));

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var categoryIds = await GetCategoryWithDescendantsAsync(categoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                string search = searchString.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) || 
                                       (p.Description != null && p.Description.ToLower().Contains(search)) ||
                                       p.Code.ToLower().Contains(search));
            }

            return await query.CountAsync();
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            if (await _productRepository.ExistsAsync(p => p.Code == product.Code && p.Status != "Deleted"))
                return false;

            product.CreatedDate = DateTime.Now;
            product.UpdatedDate = DateTime.Now;
            if (string.IsNullOrEmpty(product.Status)) product.Status = "Active";

            await _productRepository.AddAsync(product);
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            var existing = await _productRepository.GetByIdAsync(product.ProductId);
            if (existing == null) return false;

            if (existing.Code != product.Code)
            {
                if (await _productRepository.ExistsAsync(p => p.Code == product.Code && p.ProductId != product.ProductId && p.Status != "Deleted"))
                    return false;
            }

            existing.Name = product.Name;
            existing.Code = product.Code;
            existing.Description = product.Description;
            existing.SellingPrice = product.SellingPrice;
            existing.StockQuantity = product.StockQuantity;
            existing.CategoryId = product.CategoryId;
            existing.Status = product.Status ?? "Active";
            existing.UpdatedDate = DateTime.Now;

            _productRepository.Update(existing);
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return false;

            product.Status = "Inactive";
            product.UpdatedDate = DateTime.Now;
            
            _productRepository.Update(product);
            await _productRepository.SaveAsync();
            return true;
        }

        public async Task<bool> AddProductImageAsync(int productId, string imageUrl, bool isPrimary)
        {
            var img = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                IsPrimary = isPrimary,
                CreatedDate = DateTime.Now
            };
            await _imageRepository.AddAsync(img);
            await _imageRepository.SaveAsync();
            return true;
        }

        public async Task<bool> RemoveProductImagesAsync(int productId)
        {
            var images = await _imageRepository.FindAsync(i => i.ProductId == productId);
            foreach (var img in images) _imageRepository.Delete(img);
            await _imageRepository.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<Product>> GetRelatedProductsAsync(int categoryId, int currentProductId, int count)
        {
            return await _productRepository.GetQueryable()
                .AsNoTracking()
                .Include(p => p.ProductImages)
                .Where(p => p.CategoryId == categoryId && p.ProductId != currentProductId && (p.Status == "Active" || p.Status == null))
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        private async Task<List<int>> GetCategoryWithDescendantsAsync(int parentId)
        {
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            var result = new List<int> { parentId };
            
            void AddChildren(int pId)
            {
                var children = allCategories.Where(c => c.ParentId == pId).Select(c => c.CategoryId).ToList();
                foreach (var childId in children)
                {
                    if (!result.Contains(childId))
                    {
                        result.Add(childId);
                        AddChildren(childId);
                    }
                }
            }
            
            AddChildren(parentId);
            return result;
        }
    }
}
