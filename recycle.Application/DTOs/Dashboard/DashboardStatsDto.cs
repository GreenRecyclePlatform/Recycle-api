using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
namespace recycle.Application.DTOs.Dashboard
{
    /// <summary>
    /// Dashboard statistics data transfer object
    /// </summary>
    public class DashboardStatsDto
    {
        /// <summary>
        /// Total number of pickup requests in the system
        /// </summary>
        public int TotalRequests { get; set; }
        /// <summary>
        /// Number of active and approved drivers
        /// </summary>
        public int ActiveDrivers { get; set; }
        /// <summary>
        /// Total number of completed pickups
        /// </summary>
        public int CompletedPickups { get; set; }
        /// <summary>
        /// Number of requests pending approval
        /// </summary>
        public int PendingApprovals { get; set; }
        /// <summary>
        /// Number of pickups completed today
        /// </summary>
        public int TodayPickups { get; set; }
        /// <summary>
        /// Total revenue generated this month
        /// </summary>
        public decimal RevenueThisMonth { get; set; }
    }
    /// <summary>
    /// Recent activity data transfer object
    /// </summary>
    public class RecentActivityDto
    {
        /// <summary>
        /// Unique identifier for the activity
        /// </summary>
        [Required]
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Type of activity: assignment, pickup, approval, registration
        /// </summary>
        [Required]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Human-readable message describing the activity
        /// </summary>
        [Required]
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Timestamp when the activity occurred
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// Emoji icon representing the activity type
        /// </summary>
        [Required]
        public string Icon { get; set; } = string.Empty;
    }
}