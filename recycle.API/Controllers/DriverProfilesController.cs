using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application;
using recycle.Application.DTOs.DriverAssignments;
using recycle.Application.Services;
using System.Security.Claims;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DriverProfilesController : ControllerBase
    {
        DriverProfileService _driverProfileService;

        public DriverProfilesController(DriverProfileService driverProfileService)
        {
            _driverProfileService = driverProfileService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetDriverProfiles()
        {
            var driverProfiles = await _driverProfileService.GetDriverProfiles();
            return Ok(driverProfiles);
        }

        // ✅✅✅ معدل: دلوقتي بيدور بالـ User ID ✅✅✅
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDriverProfileByUserId(Guid userId)
        {
            var driverProfile = await _driverProfileService.GetDriverProfileByUserId(userId);

            if (driverProfile == null)
                return NotFound(new { message = "Driver profile not found for this user" });

            return Ok(driverProfile);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> CreateDriverProfile([FromForm] DriverProfileDto driverProfileDto)
        {
            var userId = Guid.Parse(driverProfileDto.stringUserId);
            var createdDriverProfile = await _driverProfileService.CreateDriverProfile(driverProfileDto, userId);
            return Ok(new { message = "profile created" });
        }

        [HttpPut("image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UpdateDriverProfileImage([FromForm] UpdateDriverProfileImageDto newImage)
        {
            var userId = GetUserId();
            var driverprofile = await _driverProfileService.UpdateDriverProfileImage(newImage, userId);
            return Ok(driverprofile);
        }

        [HttpPut("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Driver")]
        public async Task<ActionResult> UpdateDriverAvailability([FromBody] bool isAvailable)
        {
            var userId = GetUserId();
            var updatedDriverProfile = await _driverProfileService.UpdateDriverAvailability(userId, isAvailable);
            return Ok(updatedDriverProfile);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDriverProfile(Guid id)
        {
            var result = await _driverProfileService.DeleteDriverProfile(id);
            if (!result)
                return NotFound(new { message = "Driver profile not found" });
            return NoContent();
        }

        // ✅✅✅ معدل: بيستقبل User ID ويحوله لـ Profile ID جوا ✅✅✅
        [HttpPut("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Driver")]
        public async Task<IActionResult> UpdateDriverProfile(Guid userId, [FromBody] UpdateDriverProfileDto updateDto)
        {
            // 1. جيب الـ Driver Profile الأول بالـ User ID
            var driverProfile = await _driverProfileService.GetDriverProfileByUserId(userId);

            if (driverProfile == null)
                return NotFound(new { message = "Driver profile not found for this user" });

            // 2. اعمل Update بالـ Driver Profile ID
            var result = await _driverProfileService.UpdateDriverProfile(driverProfile.Id, updateDto);

            if (result == null)
                return NotFound(new { message = "Failed to update driver profile" });

            return Ok(result);
        }
    }
}