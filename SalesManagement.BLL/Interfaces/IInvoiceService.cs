using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetInvoiceDetailsAsync(int id);
    }
}
