using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using recycle.Application.DTOs.Dashboard;
using recycle.Application.Interfaces.IRepository;
using recycle.Application.Interfaces.IService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Services
{
    /// <summary>
    /// Dashboard service implementation - handles business logic for dashboard operations
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            try
            {
                _logger.LogInformation("🔄 DashboardService: Retrieving dashboard statistics");
                var stats = await _dashboardRepository.GetDashboardStatsAsync();
                _logger.LogInformation("✅ DashboardService: Successfully retrieved dashboard statistics");
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DashboardService: Failed to retrieve dashboard statistics");
                throw new InvalidOperationException("Failed to retrieve dashboard statistics", ex);
            }
        }

        /// <inheritdoc />
        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
        {
            try
            {
                _logger.LogInformation("🔄 DashboardService: Retrieving recent activities");
                var activities = await _dashboardRepository.GetRecentActivitiesAsync();
                _logger.LogInformation("✅ DashboardService: Successfully retrieved {Count} recent activities", activities.Count);
                return activities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ DashboardService: Failed to retrieve recent activities");
                throw new InvalidOperationException("Failed to retrieve recent activities", ex);
            }
        }
    }
}