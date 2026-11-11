using SalesManagement.DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Interfaces
{
    public interface ISettingService
    {
        Task<IEnumerable<SystemSetting>> GetAllSettingsAsync();
        Task<SystemSetting?> GetSettingByKeyAsync(string key);
        Task<bool> UpdateSettingAsync(SystemSetting setting);
        Task<bool> AddSettingAsync(SystemSetting setting);
    }
}
