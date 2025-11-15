using recycle.Application.DTOs.PickupRequest;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;

namespace recycle.Application.Services;

//public class PickupRequestService : IPickupRequestService
//{
//    private readonly IPickupRequestRepository _pickupRequestRepository;
//    private readonly IMaterialRepository _materialRepository;
//    private readonly IUnitOfWork _unitOfWork;

//    public PickupRequestService(
//        IPickupRequestRepository pickupRequestRepository,
//        IMaterialRepository materialRepository,
//        IUnitOfWork unitOfWork)
//    {
//        _pickupRequestRepository = pickupRequestRepository;
//        _materialRepository = materialRepository;
//        _unitOfWork = unitOfWork;
//    }

//    public async Task<PickupRequestResponseDto?> GetByIdAsync(Guid requestId)
//    {
//        var request = await _pickupRequestRepository.GetByIdWithDetailsAsync(requestId);

//        if (request == null)
//        {
//            return null;
//        }

//        return MapToResponseDto(request);
//    }

//    public async Task<IEnumerable<PickupRequestResponseDto>> GetAllAsync()
//    {
//        var requests = await _pickupRequestRepository.GetAllAsync();
//        return requests.Select(MapToResponseDto);
//    }

//    public async Task<IEnumerable<PickupRequestResponseDto>> GetByUserIdAsync(Guid userId)
//    {
//        var requests = await _pickupRequestRepository.GetByUserIdAsync(userId);
//        return requests.Select(MapToResponseDto);
//    }

//    public async Task<IEnumerable<PickupRequestResponseDto>> GetByStatusAsync(string status)
//    {
//        var requests = await _pickupRequestRepository.GetByStatusAsync(status);
//        return requests.Select(MapToResponseDto);
//    }

//    public async Task<(IEnumerable<PickupRequestResponseDto> Requests, int TotalCount)> GetFilteredAsync(
//        PickupRequestFilterDto filter)
//    {
//        var requests = await _pickupRequestRepository.GetFilteredAsync(
//            filter.Status,
//            filter.UserId,
//            filter.FromDate,
//            filter.ToDate,
//            filter.City,
//            filter.PageNumber,
//            filter.PageSize);

//        var totalCount = await _pickupRequestRepository.GetTotalCountAsync(
//            filter.Status,
//            filter.UserId,
//            filter.FromDate,
//            filter.ToDate,
//            filter.City);

//        return (requests.Select(MapToResponseDto), totalCount);
//    }

//    public async Task<PickupRequestResponseDto> CreateAsync(Guid userId, CreatePickupRequestDto createDto)
//    {
//        // Validate preferred pickup date
//        if (createDto.PreferredPickupDate < DateTime.UtcNow.Date)
//        {
//            throw new InvalidOperationException("Preferred pickup date cannot be in the past.");
//        }

//        // Validate materials exist
//        if (createDto.Materials == null || !createDto.Materials.Any())
//        {
//            throw new InvalidOperationException("At least one material must be specified.");
//        }

//        // Calculate total estimated weight and amount
//        decimal totalEstimatedWeight = 0;
//        decimal totalAmount = 0;
//        var requestMaterials = new List<RequestMaterial>();

//        foreach (var materialItem in createDto.Materials)
//        {
//            var material = await _materialRepository.GetByIdAsync(materialItem.MaterialId);

//            if (material == null || !material.IsActive)
//            {
//                throw new InvalidOperationException($"Material with ID {materialItem.MaterialId} not found or inactive.");
//            }

//            if (materialItem.EstimatedWeight <= 0)
//            {
//                throw new InvalidOperationException("Estimated weight must be greater than zero.");
//            }

//            var itemTotal = materialItem.EstimatedWeight * material.PricePerKg;
//            totalEstimatedWeight += materialItem.EstimatedWeight;
//            totalAmount += itemTotal;

//            requestMaterials.Add(new RequestMaterial
//            {
//                RequestMaterialId = Guid.NewGuid(),
//                MaterialId = materialItem.MaterialId,
//                EstimatedWeight = materialItem.EstimatedWeight,
//                PricePerKg = material.PricePerKg,
//                TotalAmount = itemTotal
//            });
//        }

//        // Create pickup request entity
//        var pickupRequest = new PickupRequest
//        {
//            UserId = userId,
//            PickupAddress = createDto.PickupAddress.Trim(),
//            City = createDto.City.Trim(),
//            PostalCode = createDto.PostalCode.Trim(),
//            PreferredPickupDate = createDto.PreferredPickupDate,
//            Notes = createDto.Notes?.Trim(),
//            TotalEstimatedWeight = totalEstimatedWeight,
//            TotalAmount = totalAmount,
//            RequestMaterials = requestMaterials
//        };

//        // Save to database
//        var createdRequest = await _pickupRequestRepository.CreateAsync(pickupRequest);
//        await _unitOfWork.SaveChangesAsync();

//        // Fetch with details to return
//        var requestWithDetails = await _pickupRequestRepository.GetByIdWithDetailsAsync(createdRequest.RequestId);
//        return MapToResponseDto(requestWithDetails!);
//    }

//    public async Task<PickupRequestResponseDto?> UpdateAsync(Guid requestId, UpdatePickupRequestDto updateDto)
//    {
//        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

//        if (existingRequest == null)
//        {
//            return null;
//        }

//        // Only allow updates if status is Pending
//        if (existingRequest.Status != "Pending")
//        {
//            throw new InvalidOperationException($"Cannot update request with status '{existingRequest.Status}'. Only 'Pending' requests can be updated.");
//        }

//        // Validate preferred pickup date
//        if (updateDto.PreferredPickupDate < DateTime.UtcNow.Date)
//        {
//            throw new InvalidOperationException("Preferred pickup date cannot be in the past.");
//        }

//        // Update fields
//        existingRequest.PickupAddress = updateDto.PickupAddress.Trim();
//        existingRequest.City = updateDto.City.Trim();
//        existingRequest.PostalCode = updateDto.PostalCode.Trim();
//        existingRequest.PreferredPickupDate = updateDto.PreferredPickupDate;
//        existingRequest.Notes = updateDto.Notes?.Trim();

//        await _pickupRequestRepository.UpdateAsync(existingRequest);
//        await _unitOfWork.SaveChangesAsync();

//        var updatedRequest = await _pickupRequestRepository.GetByIdWithDetailsAsync(requestId);
//        return MapToResponseDto(updatedRequest!);
//    }

//    public async Task<bool> DeleteAsync(Guid requestId)
//    {
//        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

//        if (existingRequest == null)
//        {
//            return false;
//        }

//        // Only allow deletion if status is Pending or Cancelled
//        if (existingRequest.Status != "Pending" && existingRequest.Status != "Cancelled")
//        {
//            throw new InvalidOperationException($"Cannot delete request with status '{existingRequest.Status}'.");
//        }

//        var result = await _pickupRequestRepository.DeleteAsync(requestId);
//        await _unitOfWork.SaveChangesAsync();

//        return result;
//    }

//    public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
//    {
//        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

//        if (existingRequest == null)
//        {
//            return false;
//        }

//        // Validate status transition
//        if (!CanChangeStatus(existingRequest.Status, newStatus))
//        {
//            throw new InvalidOperationException($"Invalid status transition from '{existingRequest.Status}' to '{newStatus}'.");
//        }

//        var result = await _pickupRequestRepository.UpdateStatusAsync(requestId, newStatus);
//        await _unitOfWork.SaveChangesAsync();

//        return result;
//    }

//    public bool CanChangeStatus(string currentStatus, string newStatus)
//    {
//        // Define valid status transitions
//        var validTransitions = new Dictionary<string, List<string>>
//        {
//            { "Pending", new List<string> { "Assigned", "Cancelled" } },
//            { "Assigned", new List<string> { "PickedUp", "Cancelled" } },
//            { "PickedUp", new List<string> { "Completed" } },
//            { "Completed", new List<string>() }, // Terminal state
//            { "Cancelled", new List<string>() }  // Terminal state
//        };

//        return validTransitions.ContainsKey(currentStatus) &&
//               validTransitions[currentStatus].Contains(newStatus);
//    }

//    // Manual mapping method
//    private PickupRequestResponseDto MapToResponseDto(PickupRequest request)
//    {
//        return new PickupRequestResponseDto
//        {
//            RequestId = request.RequestId,
//            UserId = request.UserId,
//            UserName = request.User != null ? $"{request.User.FirstName} {request.User.LastName}" : "Unknown",
//            PickupAddress = request.PickupAddress,
//            City = request.City,
//            PostalCode = request.PostalCode,
//            PreferredPickupDate = request.PreferredPickupDate,
//            Status = request.Status,
//            Notes = request.Notes,
//            TotalEstimatedWeight = request.TotalEstimatedWeight,
//            TotalAmount = request.TotalAmount,
//            CreatedAt = request.CreatedAt,
//            CompletedAt = request.CompletedAt,
//            Materials = request.RequestMaterials?.Select(rm => new MaterialItemDto
//            {
//                MaterialId = rm.MaterialId,
//                MaterialName = rm.Material?.MaterialName ?? "Unknown",
//                EstimatedWeight = rm.EstimatedWeight,
//                PricePerKg = rm.PricePerKg,
//                TotalAmount = rm.TotalAmount
//            }).ToList() ?? new List<MaterialItemDto>()
//        };
//    }
//}