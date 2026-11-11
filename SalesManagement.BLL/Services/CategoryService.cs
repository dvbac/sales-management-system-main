using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Where(c => !c.IsDeleted).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
        }

        public async Task<IEnumerable<Category>> GetAdminCategoriesAsync(string searchString, string statusFilter, string sortOrder)
        {
            var categories = await _categoryRepository.GetAllAsync();
            categories = categories.Where(c => !c.IsDeleted);

            if (!string.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) || 
                                               c.Description != null && c.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(statusFilter))
            {
                categories = categories.Where(c => c.Status == statusFilter);
            }

            categories = sortOrder switch
            {
                "name_desc" => categories.OrderByDescending(c => c.Name),
                "order" => categories.OrderBy(c => c.DisplayOrder),
                "order_desc" => categories.OrderByDescending(c => c.DisplayOrder),
                _ => categories.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name)
            };

            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<bool> AddCategoryAsync(Category category)
        {
            category.CreatedDate = DateTime.Now;
            category.UpdatedDate = DateTime.Now;
            if (string.IsNullOrEmpty(category.Status)) category.Status = "Active";
            category.IsDeleted = false;

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();
            return true;
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            var existing = await _categoryRepository.GetByIdAsync(category.CategoryId);
            if (existing == null) return false;

            existing.Name = category.Name;
            existing.Description = category.Description;
            if (category.ImageUrl != null) existing.ImageUrl = category.ImageUrl;
            existing.Status = category.Status;
            existing.DisplayOrder = category.DisplayOrder;
            existing.UpdatedDate = DateTime.Now;

            _categoryRepository.Update(existing);
            await _categoryRepository.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            category.IsDeleted = true;
            category.UpdatedDate = DateTime.Now;
            
            _categoryRepository.Update(category);
            await _categoryRepository.SaveAsync();
            return true;
        }
    }
}
