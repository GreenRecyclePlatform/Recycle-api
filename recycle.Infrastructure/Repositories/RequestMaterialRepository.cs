using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class RequestMaterialRepository : IRequestMaterialRepository
    {
        private readonly AppDbContext _context;

        public RequestMaterialRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RequestMaterial>> GetByRequestIdAsync(Guid requestId)
        {
            return await _context.RequestMaterials
                .Include(rm => rm.Material)
                .Where(rm => rm.RequestId == requestId)
                .OrderBy(rm => rm.Material.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<RequestMaterial?> GetByIdAsync(Guid id)
        {
            return await _context.RequestMaterials
                .Include(rm => rm.Material)
                .AsNoTracking()
                .FirstOrDefaultAsync(rm => rm.Id == id);
        }

        public async Task<RequestMaterial> CreateAsync(RequestMaterial requestMaterial)
        {
            var material = await _context.Materials
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == requestMaterial.MaterialId);

            if (material == null)
            {
                throw new InvalidOperationException($"Material with ID {requestMaterial.MaterialId} not found");
            }

            // Snapshot the current price
            requestMaterial.PricePerKg = material.PricePerKg;

            _context.RequestMaterials.Add(requestMaterial);
            await _context.SaveChangesAsync();

            return requestMaterial;
        }

        public async Task<IEnumerable<RequestMaterial>> CreateBulkAsync(IEnumerable<RequestMaterial> requestMaterials)
        {
            var materialsList = requestMaterials.ToList();

            // Get all material IDs
            var materialIds = materialsList.Select(rm => rm.MaterialId).Distinct().ToList();

            // Fetch all materials in one query
            var materials = await _context.Materials
                .AsNoTracking()
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var rm in materialsList)
            {
                if (!materials.ContainsKey(rm.MaterialId))
                {
                    throw new InvalidOperationException($"Material with ID {rm.MaterialId} not found");
                }

                // Snapshot the price
                rm.PricePerKg = materials[rm.MaterialId].PricePerKg;
            }

            _context.RequestMaterials.AddRange(materialsList);
            await _context.SaveChangesAsync();

            return materialsList;
        }

        public async Task<RequestMaterial> UpdateActualWeightAsync(Guid id, decimal actualWeight, string? notes)
        {
            var requestMaterial = await _context.RequestMaterials.FindAsync(id);

            if (requestMaterial == null)
            {
                throw new InvalidOperationException($"RequestMaterial with ID {id} not found");
            }

            requestMaterial.ActualWeight = actualWeight;
            requestMaterial.TotalAmount = actualWeight * requestMaterial.PricePerKg;

            if (!string.IsNullOrWhiteSpace(notes))
            {
                requestMaterial.Notes = notes;
            }

            _context.RequestMaterials.Update(requestMaterial);
            await _context.SaveChangesAsync();

            return requestMaterial;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var requestMaterial = await _context.RequestMaterials.FindAsync(id);
            if (requestMaterial == null) return false;

            _context.RequestMaterials.Remove(requestMaterial);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(Guid requestId, Guid materialId)
        {
            return await _context.RequestMaterials
                .AnyAsync(rm => rm.RequestId == requestId && rm.MaterialId == materialId);
        }

        public async Task<decimal> CalculateTotalAmountAsync(Guid requestId)
        {
            var total = await _context.RequestMaterials
                .Where(rm => rm.RequestId == requestId && rm.TotalAmount.HasValue)
                .SumAsync(rm => rm.TotalAmount!.Value);

            return total;
        }

        public async Task<int> GetMaterialCountForRequestAsync(Guid requestId)
        {
            return await _context.RequestMaterials
                .CountAsync(rm => rm.RequestId == requestId);
        }
    }
}
