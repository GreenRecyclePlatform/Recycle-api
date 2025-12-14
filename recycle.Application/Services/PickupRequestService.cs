using recycle.Application.DTOs;
using recycle.Application.DTOs.PickupRequest;
using recycle.Application.DTOs.RequestMaterials;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;

namespace recycle.Application.Services;

public class PickupRequestService : IPickupRequestService
{
    private readonly IPickupRequestRepository _pickupRequestRepository;
    private readonly IMaterialRepository _materialRepository;
    private readonly IRepository<Address> _addressRepository;
    private readonly INotificationHubService _notificationService;
    private readonly IUnitOfWork _unitOfWork;

    public PickupRequestService(
        IPickupRequestRepository pickupRequestRepository,
        IMaterialRepository materialRepository,
        IRepository<Address> addressRepository,
        INotificationHubService notificationService,
        IUnitOfWork unitOfWork)
    {
        _pickupRequestRepository = pickupRequestRepository;
        _materialRepository = materialRepository;
        _addressRepository = addressRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PickupRequestResponseDto?> GetByIdAsync(Guid requestId)
    {
        var request = await _pickupRequestRepository.GetByIdWithDetailsAsync(requestId);

        if (request == null)
        {
            return null;
        }

        return MapToResponseDto(request);
    }

    public async Task<IEnumerable<PickupRequestResponseDto>> GetAllAsync()
    {
        var requests = await _pickupRequestRepository.GetAllAsync();
        return requests.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<PickupRequestResponseDto>> GetByUserIdAsync(Guid userId)
    {
        var requests = await _pickupRequestRepository.GetByUserIdAsync(userId);
        return requests.Select(MapToResponseDto);
    }

    public async Task<IEnumerable<WaitingRequestDto>> GetWaitingRequestsAsync(string status)
    {
        var requests = await _pickupRequestRepository.GetWaitingRequests(status);

        var requestsDto = requests.Select(request => new WaitingRequestDto
        {
            Id = request.RequestId,
            UserName = request.User != null ? $"{request.User.FirstName} {request.User.LastName}" : "Unknown",
            PreferredPickupDate = request.PreferredPickupDate,
            PreferredPickupTime = request.PreferredPickupDate,
            Address = request.Address != null ? new AddressDto
            {
                Street = request.Address.Street,
                City = request.Address.City,
                Governorate = request.Address.Governorate,
                PostalCode = request.Address.PostalCode
            } : new AddressDto(),
            phoneNumber = request.User?.PhoneNumber ?? string.Empty,
            RequestMaterials = request.RequestMaterials?.Select(rm => new WaitingRequestMaterialDto
            {
                MaterialName = rm.Material?.Name ?? "Unknown",
                EstimatedWeight = rm.EstimatedWeight
            }).ToList() ?? new List<WaitingRequestMaterialDto>(),
            TotalEstimatedWeight = request.TotalEstimatedWeight,
            Status = request.Status
        });

        return requestsDto;
    }

    public async Task<IEnumerable<PickupRequestResponseDto>> GetByStatusAsync(string status)
    {
        var requests = await _pickupRequestRepository.GetByStatusAsync(status);
        return requests.Select(MapToResponseDto);
    }

    public async Task<(IEnumerable<PickupRequestResponseDto> Requests, int TotalCount)> GetFilteredAsync(
        PickupRequestFilterDto filter)
    {
        var requests = await _pickupRequestRepository.GetFilteredAsync(
            filter.Status,
            filter.UserId,
            filter.FromDate,
            filter.ToDate,
            filter.Governorate,
            filter.PageNumber,
            filter.PageSize);

        var totalCount = await _pickupRequestRepository.GetTotalCountAsync(
            filter.Status,
            filter.UserId,
            filter.FromDate,
            filter.ToDate,
            filter.Governorate);

        return (requests.Select(MapToResponseDto), totalCount);
    }

    public async Task<PickupRequestResponseDto> CreateAsync(Guid userId, CreatePickupRequestDto createDto)
    {
        // Validate preferred pickup date
        if (createDto.PreferredPickupDate < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Preferred pickup date cannot be in the past.");
        }

        // Validate address exists and belongs to user
        var address = await _addressRepository.GetByIdAsync(createDto.AddressId);
        if (address == null)
        {
            throw new InvalidOperationException("Address not found.");
        }

        if (address.UserId != userId)
        {
            throw new InvalidOperationException("You can only use your own addresses.");
        }

        // Validate materials exist
        if (createDto.Materials == null || !createDto.Materials.Any())
        {
            throw new InvalidOperationException("At least one material must be specified.");
        }

        // Calculate total estimated weight and amount
        decimal totalEstimatedWeight = 0;
        decimal totalAmount = 0;
        var requestMaterials = new List<RequestMaterial>();

        foreach (var materialItem in createDto.Materials)
        {
            var material = await _materialRepository.GetByIdAsync(materialItem.MaterialId);

            if (material == null || !material.IsActive)
            {
                throw new InvalidOperationException($"Material with ID {materialItem.MaterialId} not found or inactive.");
            }

            if (materialItem.EstimatedWeight <= 0)
            {
                throw new InvalidOperationException("Estimated weight must be greater than zero.");
            }

            var itemTotal = materialItem.EstimatedWeight * material.PricePerKg;
            totalEstimatedWeight += materialItem.EstimatedWeight;
            totalAmount += itemTotal;

            requestMaterials.Add(new RequestMaterial
            {
                Id = Guid.NewGuid(),
                MaterialId = materialItem.MaterialId,
                EstimatedWeight = materialItem.EstimatedWeight,
                PricePerKg = material.PricePerKg,
                TotalAmount = itemTotal
            });
        }

        // Create pickup request entity
        var pickupRequest = new PickupRequest
        {
            RequestId = Guid.NewGuid(),
            UserId = userId,
            AddressId = createDto.AddressId,
            PreferredPickupDate = createDto.PreferredPickupDate,
            Notes = createDto.Notes?.Trim(),
            TotalEstimatedWeight = totalEstimatedWeight,
            TotalAmount = totalAmount,
            Status = "Waiting",
            CreatedAt = DateTime.UtcNow,
            RequestMaterials = requestMaterials
        };

        // Save to database
        var createdRequest = await _pickupRequestRepository.CreateAsync(pickupRequest);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"🔔 About to send notifications for request {createdRequest.RequestId}");

        // 🔔 Send notification to user
        try
        {
            await _notificationService.SendToUser(userId, new NotificationDto
            {
                Title = "Pickup Request Created ✅",
                Message = "Your pickup request has been successfully created and is waiting for admin approval.",
                Type = NotificationTypes.RequestCreated,
                RelatedEntityType = NotificationEntityTypes.PickupRequest,
                RelatedEntityId = createdRequest.RequestId,
                Priority = "Normal"
            });
            Console.WriteLine($"✅ User notification sent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send user notification: {ex.Message}");
        }

        // 🔔 Send notification to admins
        try
        {
            await _notificationService.SendToRole("Admin", new NotificationDto
            {
                Title = "New Pickup Request 📦",
                Message = $"A new pickup request is waiting for assignment. Total weight: {totalEstimatedWeight}kg, Amount: ${totalAmount:F2}",
                Type = NotificationTypes.NewRequestPending,
                RelatedEntityType = NotificationEntityTypes.PickupRequest,
                RelatedEntityId = createdRequest.RequestId,
                Priority = "High"
            });
            Console.WriteLine($"✅ Admin notification sent");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send admin notification: {ex.Message}");
        }

        // Fetch with details to return
        var requestWithDetails = await _pickupRequestRepository.GetByIdWithDetailsAsync(createdRequest.RequestId);
        return MapToResponseDto(requestWithDetails!);
    }

    public async Task<PickupRequestResponseDto?> UpdateAsync(Guid requestId, UpdatePickupRequestDto updateDto)
    {
        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

        if (existingRequest == null)
        {
            return null;
        }

        // Only allow updates if status is Waiting
        if (existingRequest.Status != "Waiting")
        {
            throw new InvalidOperationException($"Cannot update request with status '{existingRequest.Status}'. Only 'Waiting' requests can be updated.");
        }

        // Validate preferred pickup date
        if (updateDto.PreferredPickupDate < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException("Preferred pickup date cannot be in the past.");
        }

        // Validate address exists and belongs to user
        var address = await _addressRepository.GetByIdAsync(updateDto.AddressId);
        if (address == null)
        {
            throw new InvalidOperationException("Address not found.");
        }

        if (address.UserId != existingRequest.UserId)
        {
            throw new InvalidOperationException("You can only use your own addresses.");
        }

        // Update fields
        existingRequest.AddressId = updateDto.AddressId;
        existingRequest.PreferredPickupDate = updateDto.PreferredPickupDate;
        existingRequest.Notes = updateDto.Notes?.Trim();

        await _pickupRequestRepository.UpdateAsync(existingRequest);
        await _unitOfWork.SaveChangesAsync();

        var updatedRequest = await _pickupRequestRepository.GetByIdWithDetailsAsync(requestId);
        return MapToResponseDto(updatedRequest!);
    }

    public async Task<bool> DeleteAsync(Guid requestId)
    {
        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

        if (existingRequest == null)
        {
            return false;
        }

        // Only allow deletion if status is Pending or Cancelled
        if (existingRequest.Status != "Pending" && existingRequest.Status != "Cancelled")
        {
            throw new InvalidOperationException($"Cannot delete request with status '{existingRequest.Status}'.");
        }

        var result = await _pickupRequestRepository.DeleteAsync(requestId);
        await _unitOfWork.SaveChangesAsync();

        return result;
    }

    public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
    {
        Console.WriteLine($"🔄 UpdateStatusAsync called - RequestId: {requestId}, NewStatus: {newStatus}");

        // Get request with full details for notifications
        var existingRequest = await _pickupRequestRepository.GetByIdWithDetailsAsync(requestId);

        if (existingRequest == null)
        {
            Console.WriteLine($"❌ Request not found: {requestId}");
            return false;
        }

        var oldStatus = existingRequest.Status;
        Console.WriteLine($"📊 Status transition: {oldStatus} → {newStatus}");

        // Validate status transition
        if (!CanChangeStatus(oldStatus, newStatus))
        {
            throw new InvalidOperationException($"Invalid status transition from '{oldStatus}' to '{newStatus}'.");
        }

        // Update status in database
        var result = await _pickupRequestRepository.UpdateStatusAsync(requestId, newStatus);
        await _unitOfWork.SaveChangesAsync();

        Console.WriteLine($"💾 Status updated in database");

        // 🔔 Send notifications based on status change
        try
        {
            await SendStatusChangeNotifications(existingRequest, oldStatus, newStatus);
            Console.WriteLine($"✅ Notifications sent for status change");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to send notifications: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Send appropriate notifications based on status change
    /// </summary>
    private async Task SendStatusChangeNotifications(PickupRequest request, string oldStatus, string newStatus)
    {
        Console.WriteLine($"📤 SendStatusChangeNotifications - Old: {oldStatus}, New: {newStatus}");

        switch (newStatus)
        {
            case "Pending": // ✅ Admin approved the request (from Waiting)
                if (oldStatus == "Waiting")
                {
                    Console.WriteLine($"📤 Sending approval notification to user {request.UserId}");

                    try
                    {
                        await _notificationService.SendToUser(request.UserId, new NotificationDto
                        {
                            Title = "Pickup Request Approved ✅",
                            Message = "Great news! Your pickup request has been approved and is now pending driver assignment.",
                            Type = NotificationTypes.RequestCreated,
                            RelatedEntityType = NotificationEntityTypes.PickupRequest,
                            RelatedEntityId = request.RequestId,
                            Priority = "High"
                        });
                        Console.WriteLine($"✅ Approval notification sent to user");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to send approval notification: {ex.Message}");
                    }
                }
                break;

            case "Assigned": // Driver assigned
                Console.WriteLine($"📤 Sending driver assigned notification");

                await _notificationService.SendToUser(request.UserId, new NotificationDto
                {
                    Title = "Driver Assigned 🚗",
                    Message = "A driver has been assigned to your pickup request. They will contact you soon.",
                    Type = NotificationTypes.DriverAssigned,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = request.RequestId,
                    Priority = "High"
                });
                break;

            case "PickedUp": // Driver picked up materials
                Console.WriteLine($"📤 Sending pickup notification");

                await _notificationService.SendToUser(request.UserId, new NotificationDto
                {
                    Title = "Materials Picked Up 📦",
                    Message = "Your recyclable materials have been picked up successfully. Processing payment...",
                    Type = NotificationTypes.PickupCompleted,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = request.RequestId,
                    Priority = "Normal"
                });
                break;

            case "Completed": // Pickup completed and verified
                Console.WriteLine($"📤 Sending completion notifications");

                // Notify user
                await _notificationService.SendToUser(request.UserId, new NotificationDto
                {
                    Title = "Pickup Completed 🎉",
                    Message = "Your pickup has been completed successfully. Payment will be processed soon.",
                    Type = NotificationTypes.PickupCompleted,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = request.RequestId,
                    Priority = "Normal"
                });

                // Notify admin
                await _notificationService.SendToRole("Admin", new NotificationDto
                {
                    Title = "Pickup Ready for Review ✅",
                    Message = $"A pickup has been completed and is ready for review. Amount: ${request.TotalAmount:F2}",
                    Type = NotificationTypes.PickupReadyForReview,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = request.RequestId,
                    Priority = "Normal"
                });
                break;

            case "Cancelled": // Request cancelled
                Console.WriteLine($"📤 Sending cancellation notification");

                await _notificationService.SendToUser(request.UserId, new NotificationDto
                {
                    Title = "Request Cancelled ❌",
                    Message = oldStatus == "Waiting"
                        ? "Your pickup request has been rejected by admin."
                        : "Your pickup request has been cancelled.",
                    Type = NotificationTypes.RequestCancelled,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = request.RequestId,
                    Priority = "Normal"
                });
                break;

            default:
                Console.WriteLine($"⚠️ No notification configured for status: {newStatus}");
                break;
        }
    }

    public bool CanChangeStatus(string currentStatus, string newStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<string, List<string>>
        {
            { "Waiting", new List<string> { "Pending", "Cancelled" } },
            { "Pending", new List<string> { "Assigned", "Cancelled" } },
            { "Assigned", new List<string> { "PickedUp", "Pending", "Cancelled" } },
            { "PickedUp", new List<string> { "Completed" } },
            { "Completed", new List<string>() },
            { "Cancelled", new List<string>() }
        };

        return validTransitions.ContainsKey(currentStatus) &&
               validTransitions[currentStatus].Contains(newStatus);
    }

    // Manual mapping method
    private PickupRequestResponseDto MapToResponseDto(PickupRequest request)
    {
        var fullAddress = request.Address != null
            ? $"{request.Address.Street}, {request.Address.City}, {request.Address.Governorate} {request.Address.PostalCode}"
            : "Address not available";

        return new PickupRequestResponseDto
        {
            RequestId = request.RequestId,
            UserId = request.UserId,
            UserName = request.User != null ? $"{request.User.FirstName} {request.User.LastName}" : "Unknown",
            AddressId = request.AddressId,
            FullAddress = fullAddress,
            PreferredPickupDate = request.PreferredPickupDate,
            Status = request.Status,
            Notes = request.Notes,
            TotalEstimatedWeight = request.TotalEstimatedWeight,
            TotalAmount = request.TotalAmount,
            CreatedAt = request.CreatedAt,
            CompletedAt = request.CompletedAt,
            Materials = request.RequestMaterials?.Select(rm => new MaterialItemDto
            {
                MaterialId = rm.MaterialId,
                MaterialName = rm.Material?.Name ?? "Unknown",
                EstimatedWeight = rm.EstimatedWeight,
                PricePerKg = rm.PricePerKg,
                TotalAmount = rm.TotalAmount ?? 0
            }).ToList() ?? new List<MaterialItemDto>()
        };
    }
}