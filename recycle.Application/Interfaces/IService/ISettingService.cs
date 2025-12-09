using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycle.Application.DTOs.Settings;

namespace recycle.Application.Interfaces.IService
{
    public interface ISettingService
    {
        Task<Dictionary<string, Dictionary<string, string>>> GetAllSettingsAsync();
        Task<Dictionary<string, string>> GetCategorySettingsAsync(string category);
        Task UpdateCategorySettingsAsync(string category, Dictionary<string, string> settings);
    }
}