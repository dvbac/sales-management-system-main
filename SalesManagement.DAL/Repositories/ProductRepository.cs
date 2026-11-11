using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.DAL.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .ToListAsync();
        }

        public override async Task<Product?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == (int)id);
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchByNameAsync(string name)
        {
            return await _dbSet
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(name))
                .ToListAsync();
        }
    }
}
