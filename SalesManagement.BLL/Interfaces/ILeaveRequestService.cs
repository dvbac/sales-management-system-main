using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface ILeaveRequestService
    {
        Task<IEnumerable<LeaveRequest>> GetAllRequestsAsync();
        Task<bool> ApproveRequestAsync(int id);
        Task<bool> RejectRequestAsync(int id);
    }
}
