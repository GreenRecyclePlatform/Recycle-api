using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly AppDbContext _context;

        public MaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Material>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Materials.AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(m => m.IsActive);
            }

            return await query
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Material?> GetByIdAsync(Guid id)
        {
            return await _context.Materials
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Material?> GetByNameAsync(string name)
        {
            return await _context.Materials
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower());
        }

        public async Task<Material> CreateAsync(Material material)
        {
            _context.Materials.Add(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<Material> UpdateAsync(Material material)
        {
            _context.Materials.Update(material);
            await _context.SaveChangesAsync();
            return material;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null) return false;

            if (await IsUsedInRequestsAsync(id))
            {
                throw new InvalidOperationException("Cannot delete material that is used in pickup requests");
            }

            _context.Materials.Remove(material);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Materials.AnyAsync(m => m.Id == id);
        }

        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
        {
            var query = _context.Materials.Where(m => m.Name.ToLower() == name.ToLower());

            if (excludeId.HasValue)
            {
                query = query.Where(m => m.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsUsedInRequestsAsync(Guid materialId)
        {
            return await _context.RequestMaterials
                .AnyAsync(rm => rm.MaterialId == materialId);
        }

        public async Task<IEnumerable<Material>> SearchAsync(string searchTerm, bool onlyActive = true)
        {
            var query = _context.Materials.AsQueryable();

            if (onlyActive)
            {
                query = query.Where(m => m.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearchTerm = searchTerm.ToLower();
                query = query.Where(m =>
                    m.Name.ToLower().Contains(lowerSearchTerm) ||
                    (m.Description != null && m.Description.ToLower().Contains(lowerSearchTerm)));
            }

            return await query
                .OrderBy(m => m.Name)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
