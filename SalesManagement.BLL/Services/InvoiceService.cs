using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetInvoiceDetailsAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }
    }
}
