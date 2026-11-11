using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;

namespace SalesManagement.DAL.Repositories
{
    public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Employee>> GetBySearchStringAsync(string searchString)
        {
            var query = _dbSet.Include(e => e.User).Where(e => !e.IsDeleted);

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(e =>
                    e.User != null && e.User.FullName != null && e.User.FullName.Contains(searchString) ||
                    e.Position != null && e.Position.Contains(searchString));
            }
            return await query.ToListAsync();
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _dbSet.Include(e => e.User).Where(e => !e.IsDeleted).ToListAsync();
        }

        public override async Task<Employee?> GetByIdAsync(object id)
        {
            return await _dbSet
                .Include(e => e.User)
                .Include(e => e.TimeAttendances)
                .FirstOrDefaultAsync(m => m.EmployeeId == (int)id);
        }
    }
}
