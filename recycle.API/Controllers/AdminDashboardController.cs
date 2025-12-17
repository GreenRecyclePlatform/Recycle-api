using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Dashboard;
using recycle.Application.Interfaces.IService;
namespace recycle.API.Controllers
{
    /// <summary>
    /// Admin Dashboard Controller - Provides dashboard statistics and recent activities
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")] // ✅ Enable CORS
    [Authorize(Roles = "Admin")] // 🔒 Uncomment when you implement authentication
    public class AdminDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<AdminDashboardController> _logger;
        public AdminDashboardController(
            IDashboardService dashboardService,
            ILogger<AdminDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }
        /// <summary>
        /// Get dashboard statistics including total requests, active drivers, and revenue
        /// </summary>
        /// <returns>Dashboard statistics</returns>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                _logger.LogInformation("📊 Fetching dashboard statistics");
                var stats = await _dashboardService.GetDashboardStatsAsync();
                _logger.LogInformation("✅ Dashboard statistics retrieved successfully");
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving dashboard statistics");
                // ✅ Return detailed error in development, generic in production
                return StatusCode(500, new
                {
                    message = "Error retrieving dashboard statistics",
                    error = ex.Message,
                    stackTrace = ex.StackTrace // Remove in production
                });
            }
        }
        /// <summary>
        /// Get recent activities including assignments, pickups, and registrations
        /// </summary>
        /// <returns>List of recent activities</returns>
        [HttpGet("recent-activities")]
        [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities()
        {
            try
            {
                _logger.LogInformation("📋 Fetching recent activities");
                var activities = await _dashboardService.GetRecentActivitiesAsync();
                _logger.LogInformation($"✅ Retrieved {activities.Count} recent activities");
                return Ok(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving recent activities");
                return StatusCode(500, new
                {
                    message = "Error retrieving recent activities",
                    error = ex.Message,
                    stackTrace = ex.StackTrace // Remove in production
                });
            }
        }
        /// <summary>
        /// 🏥 Health check endpoint - test if API is running
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "✅ Dashboard API is running", timestamp = DateTime.Now });
        }
    }
}