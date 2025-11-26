using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _repository;

        public MaterialService(IMaterialRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(bool includeInactive = false)
        {
            var materials = await _repository.GetAllAsync(includeInactive);
            return materials.Select(MapToDto);
        }

        public async Task<MaterialDto?> GetMaterialByIdAsync(Guid id)
        {
            var material = await _repository.GetByIdAsync(id);
            return material != null ? MapToDto(material) : null;
        }

        public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialDto dto)
        {
            // Validate selling price vs buying price
            if (dto.SellingPrice <= dto.BuyingPrice)
            {
                throw new InvalidOperationException("Selling price must be greater than buying price");
            }

            // Check if name is unique
            if (!await _repository.IsNameUniqueAsync(dto.Name))
            {
                throw new InvalidOperationException($"Material with name '{dto.Name}' already exists");
            }

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Unit = dto.Unit,
                Icon = dto.Icon,
                Image = dto.Image,
                BuyingPrice = dto.BuyingPrice,
                SellingPrice = dto.SellingPrice,
                PricePerKg = dto.PricePerKg,
                Status = dto.Status,
                IsActive = dto.Status == "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = null
            };

            var createdMaterial = await _repository.CreateAsync(material);
            return MapToDto(createdMaterial);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(Guid id, UpdateMaterialDto dto)
        {
            var material = await _repository.GetByIdAsync(id);

            if (material == null)
            {
                throw new InvalidOperationException($"Material with ID {id} not found");
            }

            // Validate selling price vs buying price
            if (dto.SellingPrice <= dto.BuyingPrice)
            {
                throw new InvalidOperationException("Selling price must be greater than buying price");
            }

            // Check if name is unique (excluding current material)
            if (!await _repository.IsNameUniqueAsync(dto.Name, id))
            {
                throw new InvalidOperationException($"Another material with name '{dto.Name}' already exists");
            }

            material.Name = dto.Name;
            material.Description = dto.Description;
            material.Unit = dto.Unit;
            material.Icon = dto.Icon;
            material.BuyingPrice = dto.BuyingPrice;
            material.SellingPrice = dto.SellingPrice;
            material.PricePerKg = dto.PricePerKg;
            material.Status = dto.Status;
            material.IsActive = dto.Status == "active";
            material.UpdatedAt = DateTime.UtcNow;

            // Only update image if a new one is provided
            if (!string.IsNullOrEmpty(dto.Image))
            {
                material.Image = dto.Image;
            }

            var updatedMaterial = await _repository.UpdateAsync(material);
            return MapToDto(updatedMaterial);
        }

        public async Task<bool> DeleteMaterialAsync(Guid id)
        {
            if (!await _repository.ExistsAsync(id))
            {
                return false;
            }

            try
            {
                return await _repository.DeleteAsync(id);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm, bool onlyActive = true)
        {
            var materials = await _repository.SearchAsync(searchTerm, onlyActive);
            return materials.Select(MapToDto);
        }

        private static MaterialDto MapToDto(Material material)
        {
            return new MaterialDto
            {
                Id = material.Id,
                Name = material.Name,
                Description = material.Description,
                Unit = material.Unit,
                Icon = material.Icon,
                Image = material.Image,
                BuyingPrice = material.BuyingPrice,
                SellingPrice = material.SellingPrice,
                PricePerKg = material.PricePerKg,
                Status = material.Status,
                IsActive = material.IsActive,
                CreatedAt = material.CreatedAt,
                UpdatedAt = material.UpdatedAt
            };
        }
    }
}