using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IMaterialRepository
    {
        Task<IEnumerable<Material>> GetAllAsync(bool includeInactive = false);
        Task<Material?> GetByIdAsync(Guid id);
        Task<Material?> GetByNameAsync(string name);
        Task<Material> CreateAsync(Material material);
        Task<Material> UpdateAsync(Material material);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null);
        Task<bool> IsUsedInRequestsAsync(Guid materialId);
        Task<IEnumerable<Material>> SearchAsync(string searchTerm, bool onlyActive = true);
    }
}
