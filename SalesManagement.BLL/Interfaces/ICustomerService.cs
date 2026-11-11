using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync(string? searchString);
        Task<Customer?> GetCustomerDetailsAsync(int id);
    }
}
