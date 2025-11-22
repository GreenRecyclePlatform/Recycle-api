using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.DriverAssignments;
using recycle.Application.Interfaces.IService;
using recycle.Domain.Enums;
using System.Security.Claims;

namespace recycle.API.Controllers
{
  
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class DriverAssignmentsController : ControllerBase
        {
            private readonly IDriverAssignmentService _assignmentService;

            public DriverAssignmentsController(IDriverAssignmentService assignmentService)
            {
                _assignmentService = assignmentService;
            }

            //  get current user ID from JWT token
            private Guid GetCurrentUserId()
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw new UnauthorizedAccessException("Invalid user token");
                }
                return userId;
            }

            //Admin assigns a driver to a pickup request
            [HttpPost("assign")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> AssignDriver([FromBody] CreateDriverAssignmentDto dto)
            {
                try
                {
                 var adminId = GetCurrentUserId();
                var result = await _assignmentService.AssignDriverAsync(dto, adminId);
                    return Ok(result);
                }
                catch (Exception ex)
                {

                return BadRequest(new { message = ex.Message });

            }
        }

            // Driver accepts or rejects an assignment
            [HttpPost("respond")]
            [Authorize(Roles = "Driver")]
            public async Task<IActionResult> RespondToAssignment([FromBody] DriverResponseDto dto)
            {
                try
                {

                var driverId = GetCurrentUserId();
                var result = await _assignmentService.RespondToAssignmentAsync(
                        dto.AssignmentId,
                        dto.Action,
                        dto.Notes,
                        driverId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // Driver updates assignment status (InProgress/Completed)
            [HttpPut("update-status")]
           [Authorize(Roles = "Driver")]
            public async Task<IActionResult> UpdateStatus([FromBody] UpdateAssignmentStatusDto dto)
            {
                try
                {
                var driverId = GetCurrentUserId();

                var result = await _assignmentService.UpdateAssignmentStatusAsync(
                        dto.AssignmentId,
                        dto.Status,
                        dto.Notes,
                        driverId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            //Admin reassigns a driver
            [HttpPost("reassign")]
           [Authorize(Roles = "Admin")]
            public async Task<IActionResult> ReassignDriver([FromBody] ReassignDriverDto dto)
            {
                try
                {
                var adminId = GetCurrentUserId();

                var result = await _assignmentService.ReassignDriverAsync(
                        dto.AssignmentId,
                        dto.NewDriverId,
                        adminId,
                        dto.Reason);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            // Get assignment by ID
            [HttpGet("{assignmentId}")]
            public async Task<IActionResult> GetAssignmentById(Guid assignmentId)
            {
                try
                {
                    var result = await _assignmentService.GetAssignmentByIdAsync(assignmentId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return NotFound(new { message = ex.Message });
                }
            }

            // Get all assignments for current driver (with optional status filter)
            [HttpGet("my-assignments")]
            [Authorize(Roles = "Driver")]
            public async Task<IActionResult> GetMyAssignments([FromQuery] AssignmentStatus? status = null)
            {
                try
                {

                 var driverId = GetCurrentUserId();
                var result = await _assignmentService.GetDriverAssignmentsAsync(driverId, status);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            //Get assignment history for a pickup request
            [HttpGet("request/{requestId}/history")]
           [Authorize(Roles = "Admin")]
            public async Task<IActionResult> GetRequestHistory(Guid requestId)
            {
                try
                {
                    var result = await _assignmentService.GetRequestAssignmentHistoryAsync(requestId);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            //Get active assignment for a pickup request
            [HttpGet("request/{requestId}/active")]
            public async Task<IActionResult> GetActiveAssignment(Guid requestId)
            {
                try
                {
                    var result = await _assignmentService.GetActiveAssignmentForRequestAsync(requestId);
                    if (result == null)
                        return NotFound(new { message = "No active assignment found for this request" });

                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }


        /// <summary>
        /// Get all available drivers for assignment
        /// </summary>
        [HttpGet("available-drivers")]
      //  [Authorize(Roles = "Admin")]
      [AllowAnonymous]
        public async Task<IActionResult> GetAvailableDrivers()
        {
            try
            {
                var drivers = await _assignmentService.GetAvailableDriversAsync();
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
    
}
