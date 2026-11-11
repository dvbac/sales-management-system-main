using SalesManagement.BLL.Interfaces;
using SalesManagement.DAL.Entities;
using SalesManagement.DAL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.BLL.Services
{
    public class SettingService : ISettingService
    {
        private readonly IRepository<SystemSetting> _settingRepository;

        public SettingService(IRepository<SystemSetting> settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<IEnumerable<SystemSetting>> GetAllSettingsAsync()
        {
            return await _settingRepository.GetAllAsync();
        }

        public async Task<SystemSetting?> GetSettingByKeyAsync(string key)
        {
            var settings = await _settingRepository.GetAllAsync();
            return settings.FirstOrDefault(s => s.SettingKey == key);
        }

        public async Task<bool> UpdateSettingAsync(SystemSetting setting)
        {
            var settingInDb = await GetSettingByKeyAsync(setting.SettingKey);
            if (settingInDb == null) return false;

            settingInDb.SettingValue = setting.SettingValue;
            settingInDb.Description = setting.Description;
            settingInDb.GroupName = setting.GroupName;

            _settingRepository.Update(settingInDb);
            await _settingRepository.SaveAsync();
            return true;
        }

        public async Task<bool> AddSettingAsync(SystemSetting setting)
        {
            await _settingRepository.AddAsync(setting);
            await _settingRepository.SaveAsync();
            return true;
        }
    }
}
