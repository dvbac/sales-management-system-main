using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.DAL.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }
        
        // Implementation for specific methods if added to ICategoryRepository
    }
}
