using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.PickupRequest;
using recycle.Application.Interfaces;
using System.Security.Claims;

namespace recycle.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class PickupRequestsController : ControllerBase
{
    private readonly IPickupRequestService _pickupRequestService;

    public PickupRequestsController(IPickupRequestService pickupRequestService)
    {
        _pickupRequestService = pickupRequestService;
    }

    // GET: api/pickuprequests
    [HttpGet]
   // [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PickupRequestResponseDto>>> GetAll()
    {
        try
        {
            var requests = await _pickupRequestService.GetAllAsync();
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving pickup requests.", error = ex.Message });
        }
    }

    // GET: api/pickuprequests/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PickupRequestResponseDto>> GetById(Guid id)
    {
        try
        {
            var request = await _pickupRequestService.GetByIdAsync(id);

            if (request == null)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            // Check if user is authorized to view this request
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userRole != "Admin" && request.UserId != userId)
            {
                return Forbid();
            }

            return Ok(request);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the pickup request.", error = ex.Message });
        }
    }

    // GET: api/pickuprequests/my-requests
    [HttpGet("my-requests")]
    public async Task<ActionResult<IEnumerable<PickupRequestResponseDto>>> GetMyRequests()
    {
        try
        {
            var userId = GetCurrentUserId();
            var requests = await _pickupRequestService.GetByUserIdAsync(userId);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving your requests.", error = ex.Message });
        }
    }

    // GET: api/pickuprequests/status/{status}
    [HttpGet("status/{status}")]
   //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<PickupRequestResponseDto>>> GetByStatus(string status)
    {
        try
        {
            var validStatuses = new[] { "Pending", "Assigned", "PickedUp", "Completed", "Cancelled" };

            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var requests = await _pickupRequestService.GetByStatusAsync(status);
            return Ok(requests);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving pickup requests.", error = ex.Message });
        }
    }

    // POST: api/pickuprequests/filter
    [HttpPost("filter")]
    public async Task<ActionResult> GetFiltered([FromBody] PickupRequestFilterDto filter)
    {
        try
        {
            var userRole = GetCurrentUserRole();

            // Non-admin users can only filter their own requests
            if (userRole != "Admin")
            {
                filter.UserId = GetCurrentUserId();
            }

            var (requests, totalCount) = await _pickupRequestService.GetFilteredAsync(filter);

            return Ok(new
            {
                data = requests,
                totalCount = totalCount,
                pageNumber = filter.PageNumber,
                pageSize = filter.PageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while filtering pickup requests.", error = ex.Message });
        }
    }

    // POST: api/pickuprequests
    [HttpPost]
    public async Task<ActionResult<PickupRequestResponseDto>> Create([FromBody] CreatePickupRequestDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var request = await _pickupRequestService.CreateAsync(userId, createDto);

            return CreatedAtAction(nameof(GetById), new { id = request.RequestId }, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the pickup request.", error = ex.Message });
        }
    }

    // PUT: api/pickuprequests/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PickupRequestResponseDto>> Update(Guid id, [FromBody] UpdatePickupRequestDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user owns this request
            var existingRequest = await _pickupRequestService.GetByIdAsync(id);
            if (existingRequest == null)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userRole != "Admin" && existingRequest.UserId != userId)
            {
                return Forbid();
            }

            var updatedRequest = await _pickupRequestService.UpdateAsync(id, updateDto);

            if (updatedRequest == null)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            return Ok(updatedRequest);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the pickup request.", error = ex.Message });
        }
    }

    // DELETE: api/pickuprequests/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            // Check if user owns this request
            var existingRequest = await _pickupRequestService.GetByIdAsync(id);
            if (existingRequest == null)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            if (userRole != "Admin" && existingRequest.UserId != userId)
            {
                return Forbid();
            }

            var result = await _pickupRequestService.DeleteAsync(id);

            if (!result)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the pickup request.", error = ex.Message });
        }
    }

    // PATCH: api/pickuprequests/{id}/status
    [HttpPatch("{id:guid}/status")]
  // [Authorize(Roles = "Admin,Driver")]
    public async Task<ActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusDto statusDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _pickupRequestService.UpdateStatusAsync(id, statusDto.NewStatus);

            if (!result)
            {
                return NotFound(new { message = "Pickup request not found." });
            }

            return Ok(new { message = "Status updated successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the status.", error = ex.Message });
        }
    }

    // Helper methods to get current user info from JWT claims
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
    }
}

// DTO for status update
public class UpdateStatusDto
{
    public string NewStatus { get; set; } = string.Empty;
}