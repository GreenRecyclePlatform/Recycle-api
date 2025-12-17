using Microsoft.EntityFrameworkCore;
using recycle.Application.Common;
using recycle.Application.DTOs;
using recycle.Application.DTOs.DriverAssignments;
using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    public class DriverAssignmentService : IDriverAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubService _notificationService;

        public DriverAssignmentService(IUnitOfWork unitOfWork, INotificationHubService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        //1 - Assign Driver
        public async Task<DriverAssignmentResponseDto> AssignDriverAsync(CreateDriverAssignmentDto dto, Guid adminId)
        {
            // 1. Validate pickup request exists
            var request = await _unitOfWork.PickupRequests.GetAsync(
                filter: r => r.RequestId == dto.RequestId
            );

            if (request == null)
                throw new Exception("Pickup request not found");

            // 2. Check if request is in valid status for assignment
            if (request.Status != "Pending")
                throw new Exception($"Cannot assign driver. Request status is: {request.Status}");

            // 3. ✅ Validate driver exists by DriverProfile ID
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.Id == dto.DriverId,
                includes: query => query.Include(dp => dp.User)
            );

            if (driverProfile == null)
                throw new Exception("Driver profile not found");

            if (!driverProfile.IsAvailable)
                throw new Exception("Driver is not available");

            // 4. Check if there's already an active assignment
            var existingAssignment = await _unitOfWork.DriverAssignments.GetActiveByRequestIdAsync(dto.RequestId);
            if (existingAssignment != null)
                throw new Exception("This request already has an active driver assignment");

            // 5. ✅ Create new assignment with DriverProfile ID
            var assignment = new DriverAssignment
            {
                AssignmentId = Guid.NewGuid(),
                RequestId = dto.RequestId,
                DriverId = dto.DriverId,
                AssignedByAdminId = adminId,
                AssignedAt = DateTime.UtcNow,
                Status = AssignmentStatus.Assigned,
                IsActive = true
            };

            await _unitOfWork.DriverAssignments.AddAsync(assignment);

            // 6. Update pickup request status
            request.Status = "Assigned";
            await _unitOfWork.PickupRequests.UpdateAsync(request);

            await _unitOfWork.SaveChangesAsync();

            // 🔔 Send notification to user
            await _notificationService.SendToUser(request.UserId, new NotificationDto
            {
                Title = "Driver Assigned",
                Message = $"A driver has been assigned to your pickup request.",
                Type = NotificationTypes.DriverAssigned,
                RelatedEntityType = NotificationEntityTypes.DriverAssignment,
                RelatedEntityId = assignment.AssignmentId
            });

            // 🔔 Send notification to driver
            if (driverProfile.User != null)
            {
                await _notificationService.SendToUser(driverProfile.UserId, new NotificationDto
                {
                    Title = "New Assignment",
                    Message = "You have been assigned a new pickup request.",
                    Type = NotificationTypes.NewAssignment,
                    RelatedEntityType = NotificationEntityTypes.PickupRequest,
                    RelatedEntityId = dto.RequestId
                });
            }

            // 7. Return response (reload with includes)
            var savedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignment.AssignmentId);
            return MapToResponseDto(savedAssignment);
        }

        //2 - Respond to Assignment
        public async Task<DriverAssignmentResponseDto> RespondToAssignmentAsync(Guid assignmentId, DriverAction action, string? notes, Guid driverId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            // ✅ Verify this assignment belongs to the driver (driverId is User ID from JWT)
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.UserId == driverId
            );

            if (driverProfile == null || assignment.DriverId != driverProfile.Id)
                throw new Exception("This assignment does not belong to you");

            if (assignment.Status != AssignmentStatus.Assigned)
                throw new Exception($"Cannot respond. Assignment status is: {assignment.Status}");

            if (action == DriverAction.Accept)
            {
                assignment.Status = AssignmentStatus.InProgress;
                assignment.AcceptedAt = DateTime.UtcNow;
                assignment.DriverNotes = notes;
                assignment.StartedAt = DateTime.UtcNow;

                var request = await _unitOfWork.PickupRequests.GetAsync(
                    filter: r => r.RequestId == assignment.RequestId
                );

                if (request != null)
                {
                    request.Status = "PickedUp";
                    await _unitOfWork.PickupRequests.UpdateAsync(request);

                    // 🔔 Send notification to user - driver is on the way
                    await _notificationService.SendToUser(request.UserId, new NotificationDto
                    {
                        Title = "Driver En Route",
                        Message = "Your driver is on the way to your pickup location.",
                        Type = NotificationTypes.DriverEnRoute,
                        RelatedEntityType = NotificationEntityTypes.DriverAssignment,
                        RelatedEntityId = assignmentId
                    });
                }
            }
            else if (action == DriverAction.Reject)
            {
                assignment.Status = AssignmentStatus.Rejected;
                assignment.RejectedAt = DateTime.UtcNow;
                assignment.DriverNotes = notes;
                assignment.IsActive = false;

                var request = await _unitOfWork.PickupRequests.GetAsync(
                    filter: r => r.RequestId == assignment.RequestId
                );

                if (request != null)
                {
                    request.Status = "Pending";
                    await _unitOfWork.PickupRequests.UpdateAsync(request);

                    // 🔔 Send notification to admins about rejection
                    await _notificationService.SendToRole("Admin", new NotificationDto
                    {
                        Title = "Driver Rejected Assignment",
                        Message = $"Driver rejected pickup request assignment. Reason: {notes ?? "No reason provided"}",
                        Type = NotificationTypes.DriverRejectedAssignment,
                        RelatedEntityType = NotificationEntityTypes.DriverAssignment,
                        RelatedEntityId = assignmentId
                    });
                }
            }

            await _unitOfWork.DriverAssignments.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // Reload with includes
            var updatedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            return MapToResponseDto(updatedAssignment);
        }

        //3 - Update Assignment Status
        public async Task<DriverAssignmentResponseDto> UpdateAssignmentStatusAsync(Guid assignmentId, AssignmentUpdateStatus status, string? notes, Guid driverId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            // Verify ownership (driverId is User ID from JWT)
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.UserId == driverId
            );

            if (driverProfile == null || assignment.DriverId != driverProfile.Id)
                throw new Exception("This assignment does not belong to you");

            if (assignment.Status != AssignmentStatus.InProgress)
                throw new Exception($"Cannot update. Assignment status is: {assignment.Status}");

            // Update status
            if (status == AssignmentUpdateStatus.Completed)
            {
                assignment.Status = AssignmentStatus.Completed;
                assignment.CompletedAt = DateTime.UtcNow;
                assignment.IsActive = false;
                if (!string.IsNullOrEmpty(notes))
                    assignment.DriverNotes = notes;

                var request = await _unitOfWork.PickupRequests.GetAsync(
                    filter: r => r.RequestId == assignment.RequestId
                );

                if (request != null)
                {
                    request.Status = "Completed";
                    request.CompletedAt = DateTime.UtcNow;
                    await _unitOfWork.PickupRequests.UpdateAsync(request);

                    // 🔔 Send notification to user about pickup completion
                    await _notificationService.SendToUser(request.UserId, new NotificationDto
                    {
                        Title = "Pickup Completed",
                        Message = "Your pickup has been completed successfully. Please leave a review!",
                        Type = NotificationTypes.PickupCompleted,
                        RelatedEntityType = NotificationEntityTypes.PickupRequest,
                        RelatedEntityId = request.RequestId
                    });

                    // 🔔 Send notification to admins for review
                    await _notificationService.SendToRole("Admin", new NotificationDto
                    {
                        Title = "Pickup Ready for Review",
                        Message = "A pickup has been completed and is ready for payment approval.",
                        Type = NotificationTypes.PickupReadyForReview,
                        RelatedEntityType = NotificationEntityTypes.PickupRequest,
                        RelatedEntityId = request.RequestId
                    });
                }

                // ✅ Update driver's total trips using DriverProfile
                if (driverProfile != null)
                {
                    driverProfile.TotalTrips++;
                    await _unitOfWork.DriverProfiles.UpdateAsync(driverProfile);
                }
            }

            await _unitOfWork.DriverAssignments.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            var updatedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            return MapToResponseDto(updatedAssignment);
        }

        //4 - Reassign Driver
        public async Task<DriverAssignmentResponseDto> ReassignDriverAsync(Guid assignmentId, Guid newDriverId, Guid adminId, string? reason)
        {
            var currentAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (currentAssignment == null)
                throw new Exception("Assignment not found");

            // Get old driver profile
            var oldDriverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.Id == currentAssignment.DriverId,
                includes: query => query.Include(dp => dp.User)
            );

            //  Validate new driver by DriverProfile ID
            var newDriverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.Id == newDriverId,
                includes: query => query.Include(dp => dp.User)
            );

            if (newDriverProfile == null)
                throw new Exception("New driver profile not found");

            if (!newDriverProfile.IsAvailable)
                throw new Exception("New driver is not available");

            // Deactivate current assignment
            currentAssignment.IsActive = false;
            currentAssignment.DriverNotes = $"Reassigned: {reason}";
            await _unitOfWork.DriverAssignments.UpdateAsync(currentAssignment);

            // 🔔 Send notification to old driver about cancellation
            if (oldDriverProfile != null)
            {
                await _notificationService.SendToUser(oldDriverProfile.UserId, new NotificationDto
                {
                    Title = "Assignment Cancelled",
                    Message = $"Your assignment has been cancelled and reassigned. Reason: {reason ?? "Administrative decision"}",
                    Type = NotificationTypes.AssignmentCancelled,
                    RelatedEntityType = NotificationEntityTypes.DriverAssignment,
                    RelatedEntityId = assignmentId
                });
            }

            // ✅ Create new assignment with DriverProfile ID
            var newAssignment = new DriverAssignment
            {
                AssignmentId = Guid.NewGuid(),
                RequestId = currentAssignment.RequestId,
                DriverId = newDriverId,
                AssignedByAdminId = adminId,
                AssignedAt = DateTime.UtcNow,
                Status = AssignmentStatus.Assigned,
                IsActive = true,
                DriverNotes = "Reassigned from previous driver"
            };

            await _unitOfWork.DriverAssignments.AddAsync(newAssignment);

            // Update pickup request status back to Assigned
            var request = await _unitOfWork.PickupRequests.GetAsync(
                filter: r => r.RequestId == currentAssignment.RequestId
            );

            if (request != null)
            {
                request.Status = "Assigned";
                await _unitOfWork.PickupRequests.UpdateAsync(request);

                // 🔔 Send notification to user about driver change
                await _notificationService.SendToUser(request.UserId, new NotificationDto
                {
                    Title = "Driver Changed",
                    Message = "Your pickup request has been assigned to a new driver.",
                    Type = NotificationTypes.DriverAssigned,
                    RelatedEntityType = NotificationEntityTypes.DriverAssignment,
                    RelatedEntityId = newAssignment.AssignmentId
                });
            }

            // 🔔 Send notification to new driver
            await _notificationService.SendToUser(newDriverProfile.UserId, new NotificationDto
            {
                Title = "New Assignment",
                Message = "You have been assigned a new pickup request.",
                Type = NotificationTypes.NewAssignment,
                RelatedEntityType = NotificationEntityTypes.PickupRequest,
                RelatedEntityId = currentAssignment.RequestId
            });

            await _unitOfWork.SaveChangesAsync();

            var savedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(newAssignment.AssignmentId);
            return MapToResponseDto(savedAssignment);
        }

        //5 - Get Assignment By ID
        public async Task<DriverAssignmentResponseDto> GetAssignmentByIdAsync(Guid assignmentId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            return MapToResponseDto(assignment);
        }

        //6 - Get Driver Assignments
        public async Task<List<DriverAssignmentResponseDto>> GetDriverAssignmentsAsync(Guid driverId, AssignmentStatus? status = null)
        {
            // driverId here is User ID from JWT, need to get DriverProfile ID
            var driverProfile = await _unitOfWork.DriverProfiles.GetAsync(
                filter: dp => dp.UserId == driverId
            );

            if (driverProfile == null)
                throw new Exception("Driver profile not found");

            var assignments = await _unitOfWork.DriverAssignments.GetByDriverIdAsync(driverProfile.Id, status);

            var responseDtos = new List<DriverAssignmentResponseDto>();
            foreach (var assignment in assignments)
            {
                responseDtos.Add(MapToResponseDto(assignment));
            }

            return responseDtos;
        }

        //7 - Get Request Assignment History
        public async Task<List<DriverAssignmentResponseDto>> GetRequestAssignmentHistoryAsync(Guid requestId)
        {
            var assignments = await _unitOfWork.DriverAssignments.GetHistoryByRequestIdAsync(requestId);

            var responseDtos = new List<DriverAssignmentResponseDto>();
            foreach (var assignment in assignments)
            {
                responseDtos.Add(MapToResponseDto(assignment));
            }

            return responseDtos;
        }

        //8 - Get Active Assignment For Request
        public async Task<DriverAssignmentResponseDto?> GetActiveAssignmentForRequestAsync(Guid requestId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetActiveByRequestIdAsync(requestId);
            if (assignment == null)
                return null;

            return MapToResponseDto(assignment);
        }

        //9 - Map To Response DTO
        private DriverAssignmentResponseDto MapToResponseDto(DriverAssignment assignment)
        {
            // 1
            string pickupAddress = assignment.PickupRequest?.Address != null
                ? $"{assignment.PickupRequest.Address.Street}, {assignment.PickupRequest.Address.City}"
                : "Address not available";

            // 2 -  Get driver name from DriverProfile navigation
            string driverName = assignment.Driver?.User != null
                ? $"{assignment.Driver.User.FirstName} {assignment.Driver.User.LastName}"
                : "Unknown Driver";

            // 3
            string adminName = assignment.AssignedByAdmin != null
                ? $"{assignment.AssignedByAdmin.FirstName} {assignment.AssignedByAdmin.LastName}"
                : "Unknown Admin";

            return new DriverAssignmentResponseDto
            {
                AssignmentId = assignment.AssignmentId,
                RequestId = assignment.RequestId,
                PickupAddress = pickupAddress,
                DriverId = assignment.DriverId,
                DriverName = driverName,
                AdminName = adminName,
                Status = assignment.Status,
                AssignedAt = assignment.AssignedAt,
                AcceptedAt = assignment.AcceptedAt,
                CompletedAt = assignment.CompletedAt,
                DriverNotes = assignment.DriverNotes,
                IsActive = assignment.IsActive
            };
        }

        //10 - Get Available Drivers
        public async Task<List<AvailableDriverDto>> GetAvailableDriversAsync()
        {
            // Get all users with driver profile and available
            var drivers = await _unitOfWork.Users.GetAll(
                filter: u => u.DriverProfile != null && u.DriverProfile.IsAvailable,
                includes: query => query
                    .Include(u => u.DriverProfile)
                    .Include(u => u.Addresses)
            );

            // Map to DTO
            var availableDrivers = drivers.Select(d => new AvailableDriverDto
            {
                DriverId = d.Id,
                DriverName = $"{d.FirstName} {d.LastName}",
                Email = d.Email,
                PhoneNumber = d.PhoneNumber,
                ProfileImageUrl = d.DriverProfile.profileImageUrl,
                Rating = d.DriverProfile.Rating,
                RatingCount = d.DriverProfile.ratingCount,
                IsAvailable = d.DriverProfile.IsAvailable,
                TotalTrips = d.DriverProfile.TotalTrips,
                Address = d.Addresses?.FirstOrDefault() != null
                    ? new AddressDto
                    {
                        Street = d.Addresses.First().Street,
                        City = d.Addresses.First().City,
                        Governorate = d.Addresses.First().Governorate,
                        PostalCode = d.Addresses.First().PostalCode
                    }
                    : null
            }).ToList();

            return availableDrivers;
        }
    }
}