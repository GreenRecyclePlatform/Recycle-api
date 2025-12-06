using Microsoft.AspNetCore.Http;
using recycle.Application.DTOs.Materials;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class MaterialService : IMaterialService
    {
        private readonly IMaterialRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MaterialService(IMaterialRepository repository, IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
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
            string? imageUrl = null;
            string? imageLocalPath = null;

            if (dto.Image != null)
            {
                string filename = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                string imagepath = @"wwwroot\images\materials\" + filename;
                var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), imagepath);
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\materials\"));

                using (var filestream = new FileStream(directoryLocation, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(filestream);
                }

                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}";
                imageUrl = baseUrl + "/images/materials/" + filename;
                imageLocalPath = imagepath;
            }

            if (dto.SellingPrice <= dto.BuyingPrice)
                throw new InvalidOperationException("Selling price must be greater than buying price");

            // ❌ REMOVED: Name uniqueness check
            // if (!await _repository.IsNameUniqueAsync(dto.Name))
            //     throw new InvalidOperationException($"Material with name '{dto.Name}' already exists");

            var material = new Material
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Unit = dto.Unit,
                Icon = dto.Icon,
                ImageUrl = imageUrl,
                ImageLocalPath = imageLocalPath,
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
                throw new InvalidOperationException($"Material with ID {id} not found");

            // If there's a new image, upload it and delete the old one
            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(material.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), material.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists) file.Delete();
                }

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(dto.Image.FileName);
                string imagepath = @"wwwroot\images\materials\" + filename;
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\materials\"));

                using (var filestream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imagepath), FileMode.Create))
                {
                    await dto.Image.CopyToAsync(filestream);
                }

                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}";
                material.ImageUrl = baseUrl + "/images/materials/" + filename;
                material.ImageLocalPath = imagepath;
            }

            // Validate prices
            if (dto.SellingPrice <= dto.BuyingPrice)
                throw new InvalidOperationException("Selling price must be greater than buying price");

            // ❌ REMOVED: Name uniqueness check
            // if (!await _repository.IsNameUniqueAsync(dto.Name, id))
            //     throw new InvalidOperationException($"Another material with name '{dto.Name}' already exists");

            // Update all other data
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

            var updatedMaterial = await _repository.UpdateAsync(material);
            return MapToDto(updatedMaterial);
        }

        public async Task<Material> UpdateMaterialImageAsync(Guid id, UpdateMaterialImageDto imageDto)
        {
            var material = await _repository.GetByIdAsync(id);
            if (material == null)
                throw new InvalidOperationException($"Material with ID {id} not found");

            if (imageDto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(material.ImageLocalPath))
                {
                    var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), material.ImageLocalPath);
                    FileInfo file = new FileInfo(oldFilePathDirectory);
                    if (file.Exists)
                        file.Delete();
                }

                string filename = Guid.NewGuid().ToString() + Path.GetExtension(imageDto.Image.FileName);
                string imagepath = @"wwwroot\images\materials\" + filename;
                var directoryLocation = Path.Combine(Directory.GetCurrentDirectory(), imagepath);
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\images\materials\"));

                using (var filestream = new FileStream(directoryLocation, FileMode.Create))
                {
                    await imageDto.Image.CopyToAsync(filestream);
                }

                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host.Value}{_httpContextAccessor.HttpContext.Request.PathBase.Value}";
                material.ImageUrl = baseUrl + "/images/materials/" + filename;
                material.ImageLocalPath = imagepath;
                material.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(material);
                return material;
            }

            return material;
        }

        public async Task<bool> DeleteMaterialAsync(Guid id)
        {
            var material = await _repository.GetByIdAsync(id);
            if (material == null) return false;

            if (!string.IsNullOrEmpty(material.ImageLocalPath))
            {
                var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), material.ImageLocalPath);
                FileInfo file = new FileInfo(oldFilePathDirectory);
                if (file.Exists) file.Delete();
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
                ImageUrl = material.ImageUrl,
                ImageLocalPath = material.ImageLocalPath,
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