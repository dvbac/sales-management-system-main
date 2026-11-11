using SalesManagement.DAL.Entities;

namespace SalesManagement.DAL.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        // Add specific methods here if needed, otherwise it inherits generic ones
    }
}
