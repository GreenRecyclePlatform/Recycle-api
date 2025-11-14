using recycle.Application.DTOs.RequestMaterials;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycle.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Application.Services
{

    public class RequestMaterialService : IRequestMaterialService
    {
        private readonly IRequestMaterialRepository _requestMaterialRepository;
        private readonly IMaterialRepository _materialRepository;

        public RequestMaterialService(
            IRequestMaterialRepository requestMaterialRepository,
            IMaterialRepository materialRepository)
        {
            _requestMaterialRepository = requestMaterialRepository;
            _materialRepository = materialRepository;
        }

        public async Task<IEnumerable<RequestMaterialDto>> GetRequestMaterialsAsync(Guid requestId)
        {
            var requestMaterials = await _requestMaterialRepository.GetByRequestIdAsync(requestId);
            return requestMaterials.Select(MapToDto);
        }

        public async Task<RequestMaterialDto?> GetRequestMaterialByIdAsync(Guid id)
        {
            var requestMaterial = await _requestMaterialRepository.GetByIdAsync(id);
            return requestMaterial == null ? null : MapToDto(requestMaterial);
        }

        public async Task<RequestMaterialDto> AddMaterialToRequestAsync(Guid requestId, RequestMaterialDto dto)
        {
            // Business Logic: Validate material exists and is active
            var material = await _materialRepository.GetByIdAsync(dto.MaterialId);
            if (material == null)
            {
                throw new InvalidOperationException($"Material with ID {dto.MaterialId} not found");
            }

            if (!material.IsActive)
            {
                throw new InvalidOperationException($"Material '{material.Name}' is not active");
            }

            // Business Logic: Check if material already exists in this request
            if (await _requestMaterialRepository.ExistsAsync(requestId, dto.MaterialId))
            {
                throw new InvalidOperationException($"Material '{material.Name}' is already added to this request");
            }

            // Manual Mapping: DTO to Entity
            var requestMaterial = new RequestMaterial
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                MaterialId = dto.MaterialId,
                EstimatedWeight = dto.EstimatedWeight,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
                // PricePerKg will be set in repository
            };

            var created = await _requestMaterialRepository.CreateAsync(requestMaterial);

            // Reload with Material navigation property
            var result = await _requestMaterialRepository.GetByIdAsync(created.Id);
            return MapToDto(result!);
        }

        public async Task<IEnumerable<RequestMaterialDto>> AddMaterialsToRequestAsync(
            Guid requestId,
            IEnumerable<RequestMaterialDto> dtos)
        {
            var dtosList = dtos.ToList();

            // Business Logic: Validate no duplicates in the input
            var duplicates = dtosList
                .GroupBy(d => d.MaterialId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                throw new InvalidOperationException("Duplicate materials detected in the request");
            }

            // Business Logic: Validate all materials exist and are active
            var materialIds = dtosList.Select(d => d.MaterialId).ToList();
            var materials = new Dictionary<Guid, Material>();

            foreach (var materialId in materialIds)
            {
                var material = await _materialRepository.GetByIdAsync(materialId);
                if (material == null)
                {
                    throw new InvalidOperationException($"Material with ID {materialId} not found");
                }

                if (!material.IsActive)
                {
                    throw new InvalidOperationException($"Material '{material.Name}' is not active");
                }

                // Business Logic: Check if already exists in request
                if (await _requestMaterialRepository.ExistsAsync(requestId, materialId))
                {
                    throw new InvalidOperationException($"Material '{material.Name}' is already in this request");
                }

                materials[materialId] = material;
            }

            // Manual Mapping: DTOs to Entities
            var requestMaterials = dtosList.Select(dto => new RequestMaterial
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                MaterialId = dto.MaterialId,
                EstimatedWeight = dto.EstimatedWeight,
                Notes = dto.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _requestMaterialRepository.CreateBulkAsync(requestMaterials);

            // Reload with Material navigation properties
            var result = await _requestMaterialRepository.GetByRequestIdAsync(requestId);
            return result.Select(MapToDto);
        }

        public async Task<RequestMaterialDto> UpdateActualWeightAsync(Guid id, UpdateActualWeightDto dto)
        {
            // Business Logic: Validate request material exists
            var requestMaterial = await _requestMaterialRepository.GetByIdAsync(id);
            if (requestMaterial == null)
            {
                throw new InvalidOperationException($"RequestMaterial with ID {id} not found");
            }

            // Business Logic: Prevent updating if already has actual weight (optional business rule)
            // Uncomment if you want to prevent re-updates
            // if (requestMaterial.ActualWeight.HasValue)
            // {
            //     throw new InvalidOperationException("Actual weight has already been set for this material");
            // }

            var updated = await _requestMaterialRepository.UpdateActualWeightAsync(
                id,
                dto.ActualWeight,
                dto.Notes?.Trim());

            // Reload with navigation properties
            var result = await _requestMaterialRepository.GetByIdAsync(updated.Id);
            return MapToDto(result!);
        }

        public async Task<bool> RemoveMaterialFromRequestAsync(Guid id)
        {
            // Business Logic: Validate request material exists
            var requestMaterial = await _requestMaterialRepository.GetByIdAsync(id);
            if (requestMaterial == null)
            {
                throw new InvalidOperationException($"RequestMaterial with ID {id} not found");
            }

            // Business Logic: Prevent removal if actual weight is set (optional business rule)
            if (requestMaterial.ActualWeight.HasValue)
            {
                throw new InvalidOperationException(
                    "Cannot remove material after actual weight has been recorded");
            }

            return await _requestMaterialRepository.DeleteAsync(id);
        }

        public async Task<decimal> GetRequestTotalAmountAsync(Guid requestId)
        {
            return await _requestMaterialRepository.CalculateTotalAmountAsync(requestId);
        }

        // Manual Mapping Helper: Entity to DTO
        private static RequestMaterialDto MapToDto(RequestMaterial requestMaterial)
        {
            return new RequestMaterialDto
            {
                Id = requestMaterial.Id,
                RequestId = requestMaterial.RequestId,
                MaterialId = requestMaterial.MaterialId,
                MaterialName = requestMaterial.Material?.Name ?? string.Empty,
                EstimatedWeight = requestMaterial.EstimatedWeight,
                ActualWeight = requestMaterial.ActualWeight,
                PricePerKg = requestMaterial.PricePerKg,
                TotalAmount = requestMaterial.TotalAmount,
                Notes = requestMaterial.Notes,
                CreatedAt = requestMaterial.CreatedAt
            };
        }
    }
}
