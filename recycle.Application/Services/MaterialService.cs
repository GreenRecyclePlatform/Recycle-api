using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycle.Application.DTOs.RequestMaterials;
using recycle.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _materialRepository;

        public MaterialService(IMaterialRepository materialRepository)
        {
            _materialRepository = materialRepository;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(bool includeInactive = false)
        {
            var materials = await _materialRepository.GetAllAsync(includeInactive);
            return materials.Select(MapToDto);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(Guid id)
        {
            var material = await _materialRepository.GetByIdAsync(id);
            return material == null ? null : MapToDto(material);
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto dto)
        {
            // Validate unique name
            if (!await _materialRepository.IsNameUniqueAsync(dto.Name))
            {
                throw new InvalidOperationException($"Material with name '{dto.Name}' already exists");
            }

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                Unit = dto.Unit?.Trim(),
                PricePerKg = dto.PricePerKg,
                IsActive = true,   // default for new materials
                CreatedAt = DateTime.UtcNow
            };

            var created = await _materialRepository.CreateAsync(material);
            return MapToDto(created);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(Guid id, UpdateMaterialDto dto)
        {
            var existing = await _materialRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Material with ID {id} not found");
            }

            if (!await _materialRepository.IsNameUniqueAsync(dto.Name, id))
            {
                throw new InvalidOperationException($"Material with name '{dto.Name}' already exists");
            }

            existing.Name = dto.Name.Trim();
            existing.Description = dto.Description?.Trim();
            existing.Unit = dto.Unit?.Trim();
            existing.PricePerKg = dto.PricePerKg;
            existing.IsActive = dto.IsActive;   // bool → no ??
            existing.UpdatedAt = DateTime.UtcNow;

            var updated = await _materialRepository.UpdateAsync(existing);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteMaterialAsync(Guid id)
        {
            // Business Logic: Check if material exists
            var material = await _materialRepository.GetByIdAsync(id);
            if (material == null)
            {
                throw new InvalidOperationException($"Material with ID {id} not found");
            }

            // Business Logic: Check if used in requests
            if (await _materialRepository.IsUsedInRequestsAsync(id))
            {
                throw new InvalidOperationException(
                    "Cannot delete material that is used in pickup requests. Consider deactivating it instead.");
            }

            return await _materialRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm, bool onlyActive = true)
        {
            var materials = await _materialRepository.SearchAsync(searchTerm, onlyActive);
            return materials.Select(MapToDto);
        }

        // Manual Mapping Helper: Entity to DTO
        private static MaterialDto MapToDto(Material material)
        {
            return new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Description = material.Description,
                Unit = material.Unit,
                PricePerKg = material.PricePerKg,
                IsActive = material.IsActive,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }
    }
}
