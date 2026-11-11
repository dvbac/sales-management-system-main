using SalesManagement.BLL.DTOs;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDTO> GetDashboardStatsAsync();
    }
}
