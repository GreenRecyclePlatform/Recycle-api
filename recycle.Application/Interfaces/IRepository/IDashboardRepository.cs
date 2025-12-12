using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using recycle.Application.DTOs.Dashboard;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces.IRepository
{
    /// <summary>
    /// Repository interface for dashboard data access
    /// </summary>
    public interface IDashboardRepository
    {
        /// <summary>
        /// Retrieves dashboard statistics from the database
        /// </summary>
        /// <returns>Aggregated dashboard statistics</returns>
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        /// <summary>
        /// Retrieves recent activities from the database
        /// </summary>
        /// <returns>List of recent activities limited to last 10 entries</returns>
        Task<List<RecentActivityDto>> GetRecentActivitiesAsync();
    }
}