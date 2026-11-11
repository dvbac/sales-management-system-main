using SalesManagement.DAL.Entities;

namespace SalesManagement.DAL.Interfaces
{
    public interface IEmployeeRepository : IRepository<Employee>
    {
        Task<IEnumerable<Employee>> GetBySearchStringAsync(string searchString);
    }
}
