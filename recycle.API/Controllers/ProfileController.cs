// recycle.API/Controllers/ProfileController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Notifications;
using recycle.Application.DTOs.Profile;
using recycle.Application.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        /// <summary>
        /// GET: api/Profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _profileService.GetProfileAsync(userId);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Profile
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _profileService.UpdateProfileAsync(userId, dto);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Profile/address
        /// </summary>
        [HttpPut("address")]
        public async Task<IActionResult> UpdateAddress([FromBody] UpdateAddressDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var address = await _profileService.UpdateAddressAsync(userId, dto);
                return Ok(address);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// PUT: api/Profile/notifications
        /// </summary>
        [HttpPut("notifications")]
        public async Task<IActionResult> UpdateNotificationPreferences([FromBody] NotificationPreferencesDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var preferences = await _profileService.UpdateNotificationPreferencesAsync(userId, dto);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User not authenticated");

            return Guid.Parse(userIdClaim);
        }
    }
}