using SalesManagement.DAL.Entities;

namespace SalesManagement.DAL.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
        Task<IEnumerable<Product>> SearchByNameAsync(string name);
    }
}
