using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using recycle.Application.DTOs.Dashboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IService
{
    /// <summary>
    /// Service interface for dashboard operations
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves comprehensive dashboard statistics
        /// </summary>
        /// <returns>Dashboard statistics including requests, drivers, and revenue</returns>
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        /// <summary>
        /// Retrieves recent system activities
        /// </summary>
        /// <returns>List of recent activities ordered by timestamp</returns>
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync();
    }
}