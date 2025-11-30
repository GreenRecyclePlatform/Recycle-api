using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using recycle.Infrastructure;

namespace recycle.Infrastructure.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly AppDbContext _context;

        public SettingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Setting>> GetAllAsync()
        {
            return await _context.Set<Setting>().ToListAsync();
        }

        public async Task<IEnumerable<Setting>> GetByCategoryAsync(string category)
        {
            return await _context.Set<Setting>()
                .Where(s => s.Category == category)
                .ToListAsync();
        }

        public async Task<Setting?> GetByKeyAsync(string category, string key)
        {
            return await _context.Set<Setting>()
                .FirstOrDefaultAsync(s => s.Category == category && s.Key == key);
        }

        public async Task<Setting> CreateAsync(Setting setting)
        {
            setting.CreatedAt = DateTime.UtcNow;
            _context.Set<Setting>().Add(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task<Setting> UpdateAsync(Setting setting)
        {
            setting.UpdatedAt = DateTime.UtcNow;
            _context.Set<Setting>().Update(setting);
            await _context.SaveChangesAsync();
            return setting;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var setting = await _context.Set<Setting>().FindAsync(id);
            if (setting == null) return false;

            _context.Set<Setting>().Remove(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task BulkUpdateAsync(string category, Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                var setting = await GetByKeyAsync(category, kvp.Key);

                if (setting != null)
                {
                    setting.Value = kvp.Value;
                    setting.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    setting = new Setting
                    {
                        Id = Guid.NewGuid(),
                        Category = category,
                        Key = kvp.Key,
                        Value = kvp.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Set<Setting>().Add(setting);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}