using recycle.Application.Common;
using recycle.Application.DTOs.DriverAssignments;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IService
{
  


    public interface IDriverAssignmentService
    {
        // Admin assigns driver to a pickup request
        Task<DriverAssignmentResponseDto> AssignDriverAsync(CreateDriverAssignmentDto dto, Guid adminId);

        // Driver accepts or rejects assignment
        Task<DriverAssignmentResponseDto> RespondToAssignmentAsync(Guid assignmentId, DriverAction action, string? notes, Guid driverId);

        // Driver updates assignment status (InProgress/Completed)
        Task<DriverAssignmentResponseDto> UpdateAssignmentStatusAsync(Guid assignmentId, AssignmentUpdateStatus status, string? notes, Guid driverId);

        // Admin reassigns driver
        Task<DriverAssignmentResponseDto> ReassignDriverAsync(Guid assignmentId, Guid newDriverId, Guid adminId, string? reason);

        // Get assignment by ID
        Task<DriverAssignmentResponseDto> GetAssignmentByIdAsync(Guid assignmentId);

        // Get all assignments for a specific driver
        Task<List<DriverAssignmentResponseDto>> GetDriverAssignmentsAsync(Guid driverId, AssignmentStatus? status = null);

        // Get assignment history for a pickup request
        Task<List<DriverAssignmentResponseDto>> GetRequestAssignmentHistoryAsync(Guid requestId);

        // Get active assignment for a pickup request
        Task<DriverAssignmentResponseDto?> GetActiveAssignmentForRequestAsync(Guid requestId);

        Task<List<AvailableDriverDto>> GetAvailableDriversAsync();

    }
}










