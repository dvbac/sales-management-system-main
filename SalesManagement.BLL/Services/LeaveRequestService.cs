using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;

namespace SalesManagement.BLL.Services
{
    public class LeaveRequestService : ILeaveRequestService
    {
        private readonly IRepository<LeaveRequest> _requestRepository;
        private readonly AppDbContext _context;

        public LeaveRequestService(IRepository<LeaveRequest> requestRepository, AppDbContext context)
        {
            _requestRepository = requestRepository;
            _context = context;
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync()
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .ThenInclude(e => e.User)
                .OrderByDescending(l => l.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ApproveRequestAsync(int id)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return false;

            request.Status = "Approved";
            _requestRepository.Update(request);
            await _requestRepository.SaveAsync();
            return true;
        }

        public async Task<bool> RejectRequestAsync(int id)
        {
            var request = await _requestRepository.GetByIdAsync(id);
            if (request == null) return false;

            request.Status = "Rejected";
            _requestRepository.Update(request);
            await _requestRepository.SaveAsync();
            return true;
        }
    }
}
