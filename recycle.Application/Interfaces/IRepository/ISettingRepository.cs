using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using recycle.Domain.Entities;

namespace recycle.Application.Interfaces.IRepository
{
    public interface ISettingRepository
    {
        Task<IEnumerable<Setting>> GetAllAsync();
        Task<IEnumerable<Setting>> GetByCategoryAsync(string category);
        Task<Setting?> GetByKeyAsync(string category, string key);
        Task<Setting> CreateAsync(Setting setting);
        Task<Setting> UpdateAsync(Setting setting);
        Task<bool> DeleteAsync(Guid id);
        Task BulkUpdateAsync(string category, Dictionary<string, string> settings);
    }
}