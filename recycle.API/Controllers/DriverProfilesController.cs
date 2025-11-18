using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using recycle.Application;
using recycle.Application.Services;
using System.Security.Claims;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<ActionResult> GetDriverProfiles()
        {
            var driverProfiles = await _driverProfileService.GetDriverProfiles();
            return Ok(driverProfiles);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> GetDriverProfileById(Guid id)
        {
            var driverProfile = await _driverProfileService.GetDriverProfileById(id);
            return Ok(driverProfile);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult> CreateDriverProfile([FromBody] DriverProfileDto driverProfileDto)
        {
            var userId = GetUserId();

            var createdDriverProfile = await _driverProfileService.CreateDriverProfile(driverProfileDto, userId);
            return CreatedAtAction(nameof(GetDriverProfileById), new { id = createdDriverProfile.Id }, createdDriverProfile);
        }

        [HttpPut("availability")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateDriverAvailability([FromBody] bool isAvailable)
        {
            var userId = GetUserId();
            var updatedDriverProfile = await _driverProfileService.UpdateDriverAvailability(userId, isAvailable);
            return Ok(updatedDriverProfile);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteDriverProfile()
        {
            var userId = GetUserId();
            await _driverProfileService.DeleteDriverProfile(userId);
            return NoContent();
        }

    }
}
