// recycle.API/Controllers/ProfileController.cs - FIXED VERSION
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Notifications;
using recycle.Application.DTOs.Profile;
using recycle.Application.Interfaces;
using System;
using System.IdentityModel.Tokens.Jwt;
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
        /// ✅ FIXED: Get current user's profile with better user ID extraction
        /// GET /api/profile
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // ✅ Try multiple claim types to find user ID
                var userId = GetUserIdFromToken();

                if (userId == Guid.Empty)
                {
                    return Unauthorized(new
                    {
                        message = "User ID not found in token",
                        availableClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                    });
                }

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
                var userId = GetUserIdFromToken();

                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

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
                var userId = GetUserIdFromToken();

                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

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
                var userId = GetUserIdFromToken();

                if (userId == Guid.Empty)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                var preferences = await _profileService.UpdateNotificationPreferencesAsync(userId, dto);
                return Ok(preferences);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        /// <summary>
        /// PUT: api/Profile/paypal-email
        /// </summary>
        [HttpPut("paypal-email")]
        public async Task<IActionResult> UpdatePayPalEmail([FromBody] UpdatePayPalEmailDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _profileService.UpdatePayPalEmailAsync(userId, dto.PayPalEmail);

                if (!success)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "PayPal email updated successfully" });
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

        /// <summary>
        /// ✅ Helper method to extract user ID from JWT token claims
        /// Works with your existing token structure
        /// </summary>
        private Guid GetUserIdFromToken()
        {
            // Try different possible claim types that YOUR TokenService already generates
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value  // ✅ Your token has this
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value            // ✅ Your token has this too
                ?? User.FindFirst("uid")?.Value
                ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

           
            if (string.IsNullOrEmpty(userIdString))
            {
                return Guid.Empty;
            }

            return Guid.TryParse(userIdString, out var userId) ? userId : Guid.Empty;
        }
    }
}