using Microsoft.EntityFrameworkCore;
using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.BLL.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly AppDbContext _context; // For complex includes

        public CustomerService(ICustomerRepository customerRepository, AppDbContext context)
        {
            _customerRepository = customerRepository;
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync(string? searchString)
        {
            var query = _context.Customers
                .Include(c => c.Orders)
                .OrderByDescending(c => c.CreatedDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(c => c.FullName.ToLower().Contains(searchString) || 
                                       c.PhoneNumber.Contains(searchString) || 
                                       c.Email.ToLower().Contains(searchString));
            }

            return await query.ToListAsync();
        }

        public async Task<Customer?> GetCustomerDetailsAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.User)
                .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(m => m.CustomerId == id);
        }
    }
}
