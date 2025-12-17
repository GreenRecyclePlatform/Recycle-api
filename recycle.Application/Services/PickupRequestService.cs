using Microsoft.EntityFrameworkCore;
using recycle.Application.DTOs;
using recycle.Application.DTOs.PickupRequest;
using recycle.Application.DTOs.RequestMaterials;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static System.Collections.Specialized.BitVector32;

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

        // ✅ ADD THIS SECTION AT THE VERY BEGINNING (BEFORE address validation)
        // ========================================================================
        // Validate PayPal email
        if (string.IsNullOrWhiteSpace(createDto.PayPalEmail))
        {
            throw new InvalidOperationException("PayPal email is required for payment processing.");
        }

        if (!IsValidEmail(createDto.PayPalEmail))
        {
            throw new InvalidOperationException("Invalid PayPal email format.");
        }
        // ========================================================================



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


        // ✅ ADD THIS SECTION HERE(AFTER address validation, BEFORE materials)
    // ========================================================================
    // Get user and update PayPal email
    var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        user.PayPalEmail = createDto.PayPalEmail;
        await _unitOfWork.Users.UpdateAsync(user);
        // ========================================================================



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
                Title = "Pickup Request Created",
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
                Title = "New Pickup Request",
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

    //public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
    //{
    //    var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);

    //    if (existingRequest == null)
    //    {
    //        return false;
    //    }

    //    // Validate status transition
    //    if (!CanChangeStatus(existingRequest.Status, newStatus))
    //    {
    //        throw new InvalidOperationException($"Invalid status transition from '{existingRequest.Status}' to '{newStatus}'.");
    //    }

    //    var result = await _pickupRequestRepository.UpdateStatusAsync(requestId, newStatus);
    //    await _unitOfWork.SaveChangesAsync();

    //    return result;
    //}

    public bool CanChangeStatus(string currentStatus, string newStatus)
    {
        // Define valid status transitions
        var validTransitions = new Dictionary<string, List<string>>
        {
            { "Pending", new List<string> { "Assigned", "Cancelled" } },
            { "Waiting", new List<string> { "Pending", "Cancelled" } },
            { "Assigned", new List<string> { "PickedUp", "Cancelled" } },
            { "PickedUp", new List<string> { "Completed" } },
            { "Completed", new List<string>() }, // Terminal state
            { "Cancelled", new List<string>() }  // Terminal state
        };

        return validTransitions.ContainsKey(currentStatus) &&
               validTransitions[currentStatus].Contains(newStatus);
    }


    // Add this method to your PickupRequestService.cs

public async Task<bool> UpdateStatusAsync(Guid requestId, string newStatus)
    {
        var existingRequest = await _pickupRequestRepository.GetByIdAsync(requestId);
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

        // ✅ NEW: Automatically create payment request when pickup is completed
        if (newStatus == "Completed" && result)
        {
            await CreateAutomaticPaymentRequest(existingRequest);
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    // ✅ NEW METHOD: Create payment request automatically
    private async Task CreateAutomaticPaymentRequest(PickupRequest pickupRequest)
    {
        try
        {
            // Get user with PayPal email
            var user = await _unitOfWork.Users.GetByIdAsync(pickupRequest.UserId);
            //if (user == null)
            //{
            //    _logger.LogError("User {UserId} not found for pickup request {RequestId}",
            //        pickupRequest.UserId, pickupRequest.RequestId);
            //    return;
            //}

            // Check if payment already exists for this request
            var existingPayment = await _unitOfWork.Payments.GetQueryable()
                .FirstOrDefaultAsync(p => p.RequestId == pickupRequest.RequestId);

            //if (existingPayment != null)
            //{
            //    _logger.LogInformation("Payment already exists for request {RequestId}",
            //        pickupRequest.RequestId);
            //    return;
            //}

            // Get PayPal email from user (you added this field to ApplicationUser)
            var paypalEmail = user.PayPalEmail ?? user.Email;

            //if (string.IsNullOrWhiteSpace(paypalEmail))
            //{
            //    _logger.LogWarning("No PayPal email found for user {UserId}", user.Id);
            //    // You might want to notify admin here
            //    return;
            //}

            // Create payment record
            var payment = new Payment
            {
                ID = Guid.NewGuid(),
                RequestId = pickupRequest.RequestId,
                RecipientUserID = pickupRequest.UserId,
                RecipientType = "User",
                Amount = pickupRequest.TotalAmount, // Use actual calculated amount
                PaymentMethod = paypalEmail, // Store PayPal email here
                PaymentStatus = PaymentStatuses.Pending,
                CreatedAt = DateTime.UtcNow,
                AdminNotes = $"Auto-generated payment for completed pickup request"
            };

            await _unitOfWork.Payments.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            //_logger.LogInformation(
            //    "Payment {PaymentId} created automatically for completed pickup request {RequestId}, Amount: {Amount}",
            //    payment.ID, pickupRequest.RequestId, payment.Amount);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
            //_logger.LogError(ex,
            //    "Error creating automatic payment for pickup request {RequestId}",
            //    pickupRequest.RequestId);
            // Don't throw - we don't want to fail the status update if payment creation fails
        }
    }


    // ✅ ADD THIS METHOD AT THE END OF THE CLASS (after all methods)
    // ========================================================================
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    // ========================================================================



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