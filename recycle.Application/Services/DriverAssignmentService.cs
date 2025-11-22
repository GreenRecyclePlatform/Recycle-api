using Microsoft.EntityFrameworkCore;
using recycle.Application.Common;
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

        public DriverAssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        //1
        public async Task<DriverAssignmentResponseDto> AssignDriverAsync(CreateDriverAssignmentDto dto, Guid adminId)
        {
            // 1. Validate pickup request exists - use--> GetAsync
            var request = await _unitOfWork.PickupRequests.GetAsync(
                filter: r => r.RequestId == dto.RequestId
            );

            if (request == null)
                throw new Exception("Pickup request not found");

            // 2. Check if request is in valid status for assignment
            if (request.Status != "Pending")
                throw new Exception($"Cannot assign driver. Request status is: {request.Status}");

            // 3. Validate driver exists - use--> GetAsync & include
            var driver = await _unitOfWork.Users.GetAsync(
                filter: u => u.Id == dto.DriverId,
                includes: query => query.Include(u => u.DriverProfile)
            );

            if (driver == null || driver.DriverProfile == null)
                throw new Exception("Driver not found");

            if (!driver.DriverProfile.IsAvailable)
                throw new Exception("Driver is not available");

            // 4. Check if there's already an active assignment
            var existingAssignment = await _unitOfWork.DriverAssignments.GetActiveByRequestIdAsync(dto.RequestId);
            if (existingAssignment != null)
                throw new Exception("This request already has an active driver assignment");

            // 5. Create new assignment
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

            // 7. Return response (reload with includes)
            var savedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignment.AssignmentId);
            return MapToResponseDto(savedAssignment);
        }
        //2
        public async Task<DriverAssignmentResponseDto> RespondToAssignmentAsync(Guid assignmentId, DriverAction action, string? notes, Guid driverId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            if (assignment.DriverId != driverId)
                throw new Exception("This assignment does not belong to you");

            if (assignment.Status != AssignmentStatus.Assigned)
                throw new Exception($"Cannot respond. Assignment status is: {assignment.Status}");

            if (action == DriverAction.Accept)
            {
                assignment.Status = AssignmentStatus.InProgress; 

              //  assignment.Status = AssignmentStatus.Accepted;
                assignment.AcceptedAt = DateTime.UtcNow;
                assignment.DriverNotes = notes;
                assignment.StartedAt = DateTime.UtcNow;  

                // use--> GetAsync
                var request = await _unitOfWork.PickupRequests.GetAsync(
                    filter: r => r.RequestId == assignment.RequestId
                );

                if (request != null)
                {
                    request.Status = "PickedUp"; 
                    await _unitOfWork.PickupRequests.UpdateAsync(request);
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
                }
            }

            await _unitOfWork.DriverAssignments.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            // Reload with includes
            var updatedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            return MapToResponseDto(updatedAssignment);
        }
        //3
        public async Task<DriverAssignmentResponseDto> UpdateAssignmentStatusAsync(Guid assignmentId, AssignmentUpdateStatus status, string? notes, Guid driverId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            if (assignment.DriverId != driverId)
                throw new Exception("This assignment does not belong to you");

            //if (assignment.Status != AssignmentStatus.Accepted && assignment.Status != AssignmentStatus.InProgress)
                if (assignment.Status != AssignmentStatus.InProgress)

                    throw new Exception($"Cannot update. Assignment status is: {assignment.Status}");
            //// Update status-1
            //if (status == AssignmentUpdateStatus.InProgress)
            //{
            //    assignment.Status = AssignmentStatus.InProgress;
            //    assignment.StartedAt = DateTime.UtcNow;
            //    assignment.DriverNotes = notes;

            //    var request = await _unitOfWork.PickupRequests.GetAsync(
            //        filter: r => r.RequestId == assignment.RequestId
            //    );

            //    if (request != null)
            //    {
            //        request.Status = "PickedUp";
            //        await _unitOfWork.PickupRequests.UpdateAsync(request);
            //    }
            //}
            // Update status-2
            else if (status == AssignmentUpdateStatus.Completed)
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
                }

                // Update driver's total trips
                var driver = await _unitOfWork.Users.GetAsync(
                    filter: u => u.Id == assignment.DriverId,
                    includes: query => query.Include(u => u.DriverProfile)
                );

                if (driver?.DriverProfile != null)
                {
                    driver.DriverProfile.TotalTrips++;
                    await _unitOfWork.Users.UpdateAsync(driver);
                }
            }

            await _unitOfWork.DriverAssignments.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            var updatedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            return MapToResponseDto(updatedAssignment);
        }
        //4
        public async Task<DriverAssignmentResponseDto> ReassignDriverAsync(Guid assignmentId, Guid newDriverId, Guid adminId, string? reason)
        {
            var currentAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (currentAssignment == null)
                throw new Exception("Assignment not found");

            // Validate new driver
            var newDriver = await _unitOfWork.Users.GetAsync(
                filter: u => u.Id == newDriverId,
                includes: query => query.Include(u => u.DriverProfile)
            );

            if (newDriver == null || newDriver.DriverProfile == null)
                throw new Exception("New driver not found");

            if (!newDriver.DriverProfile.IsAvailable)
                throw new Exception("New driver is not available");

            // Deactivate current assignment
            currentAssignment.IsActive = false;
            currentAssignment.DriverNotes = $"Reassigned: {reason}";
            await _unitOfWork.DriverAssignments.UpdateAsync(currentAssignment);

            // Create new assignment
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
            }

            await _unitOfWork.SaveChangesAsync();

            var savedAssignment = await _unitOfWork.DriverAssignments.GetByIdAsync(newAssignment.AssignmentId);
            return MapToResponseDto(savedAssignment);
        }
        //5
        public async Task<DriverAssignmentResponseDto> GetAssignmentByIdAsync(Guid assignmentId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetByIdAsync(assignmentId);
            if (assignment == null)
                throw new Exception("Assignment not found");

            return MapToResponseDto(assignment);
        }
        //6
        public async Task<List<DriverAssignmentResponseDto>> GetDriverAssignmentsAsync(Guid driverId, AssignmentStatus? status = null)
        {
            var assignments = await _unitOfWork.DriverAssignments.GetByDriverIdAsync(driverId, status);

            var responseDtos = new List<DriverAssignmentResponseDto>();
            foreach (var assignment in assignments)
            {
                responseDtos.Add(MapToResponseDto(assignment));
            }

            return responseDtos;
        }
        //7
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
        //8
        public async Task<DriverAssignmentResponseDto?> GetActiveAssignmentForRequestAsync(Guid requestId)
        {
            var assignment = await _unitOfWork.DriverAssignments.GetActiveByRequestIdAsync(requestId);
            if (assignment == null)
                return null;

            return MapToResponseDto(assignment);
        }
        //9
        // Helper method for manual mapping (synchronous)
        private DriverAssignmentResponseDto MapToResponseDto(DriverAssignment assignment)
        {
            //1
            string pickupAddress = assignment.PickupRequest?.Address != null
                ? $"{assignment.PickupRequest.Address.Street}, {assignment.PickupRequest.Address.City}"
                : "Address not available";
            //2
            string driverName = assignment.Driver?.User != null
                ? $"{assignment.Driver.User.FirstName} {assignment.Driver.User.LastName}"
                : "Unknown Driver";
            //3
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

        public async Task<List<AvailableDriverDto>> GetAvailableDriversAsync()
        {
            // Get all users with driver profile and available
            var drivers = await _unitOfWork.Users.GetAll(
                filter: u => u.DriverProfile != null && u.DriverProfile.IsAvailable,
                includes: query => query.Include(u => u.DriverProfile)
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
                TotalTrips = d.DriverProfile.TotalTrips
            }).ToList();

            return availableDrivers;
        }
    }
}

