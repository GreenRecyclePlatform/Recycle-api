using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using recycle.Application.DTOs.Settings;
using recycle.Application.Interfaces.IService;

namespace recycle.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // Change to [Authorize(Roles = "Admin")] when ready
    public class SettingsController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingsController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        /// <summary>
        /// Get all settings grouped by category
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetAllSettings()
        {
            try
            {
                var settings = await _settingService.GetAllSettingsAsync();
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving settings", error = ex.Message });
            }
        }

        /// <summary>
        /// Get settings for a specific category
        /// </summary>
        [HttpGet("{category}")]
        public async Task<ActionResult> GetCategorySettings(string category)
        {
            try
            {
                var settings = await _settingService.GetCategorySettingsAsync(category);
                return Ok(settings);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving settings", error = ex.Message });
            }
        }

        /// <summary>
        /// Update settings for a specific category
        /// </summary>
        [HttpPut("{category}")]
        public async Task<ActionResult> UpdateCategorySettings(
            string category,
            [FromBody] Dictionary<string, string> settings)
        {
            try
            {
                await _settingService.UpdateCategorySettingsAsync(category, settings);
                return Ok(new { message = $"{category} settings updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating settings", error = ex.Message });
            }
        }
    }
}