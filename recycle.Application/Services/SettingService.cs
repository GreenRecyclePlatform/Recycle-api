using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using recycle.Application.DTOs.Settings;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;

namespace recycle.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _settingRepository;

        public SettingService(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

        public async Task<Dictionary<string, Dictionary<string, string>>> GetAllSettingsAsync()
        {
            var settings = await _settingRepository.GetAllAsync();

            return settings
                .GroupBy(s => s.Category)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(s => s.Key, s => s.Value)
                );
        }

        public async Task<Dictionary<string, string>> GetCategorySettingsAsync(string category)
        {
            var settings = await _settingRepository.GetByCategoryAsync(category);
            return settings.ToDictionary(s => s.Key, s => s.Value);
        }

        public async Task UpdateCategorySettingsAsync(string category, Dictionary<string, string> settings)
        {
            await _settingRepository.BulkUpdateAsync(category, settings);
        }
    }
}