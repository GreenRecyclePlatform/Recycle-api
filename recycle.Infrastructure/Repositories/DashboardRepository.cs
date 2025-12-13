using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using recycle.Application.DTOs.Dashboard;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(AppDbContext context, ILogger<DashboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        //{
        //    try
        //    {
        //        _logger.LogInformation("📊 Calculating dashboard statistics");

        //        var today = DateTime.UtcNow.Date;
        //        var tomorrow = today.AddDays(1);
        //        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        //        var totalRequestsTask = _context.PickupRequests.CountAsync();
        //        var activeDriversTask = _context.DriverProfiles.Where(d => d.IsAvailable).CountAsync();
        //        var completedPickupsTask = _context.PickupRequests.Where(r => r.Status == "Completed").CountAsync();
        //        var pendingApprovalsTask = _context.PickupRequests.Where(r => r.Status == "Pending").CountAsync();
        //        var todayPickupsTask = _context.PickupRequests
        //            .Where(r => r.CreatedAt >= today && r.CreatedAt < tomorrow && r.Status == "Completed")
        //            .CountAsync();

        //        // Sum with null-safety: if TotalAmount is nullable, replace null with 0
        //        var revenueThisMonthTask = _context.PickupRequests
        //            .Where(r => r.CreatedAt >= firstDayOfMonth && r.Status == "Completed")
        //            .Select(r => (decimal?)r.TotalAmount ?? 0)
        //            .SumAsync();

        //        await Task.WhenAll(totalRequestsTask, activeDriversTask, completedPickupsTask,
        //                           pendingApprovalsTask, todayPickupsTask, revenueThisMonthTask);

        //        var stats = new DashboardStatsDto
        //        {
        //            TotalRequests = await totalRequestsTask,
        //            ActiveDrivers = await activeDriversTask,
        //            CompletedPickups = await completedPickupsTask,
        //            PendingApprovals = await pendingApprovalsTask,
        //            TodayPickups = await todayPickupsTask,
        //            RevenueThisMonth = await revenueThisMonthTask
        //        };

        //        _logger.LogInformation("✅ Dashboard statistics calculated: {TotalRequests} total, {ActiveDrivers} active",
        //            stats.TotalRequests, stats.ActiveDrivers);

        //        return stats;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "❌ Error calculating dashboard statistics");
        //        throw;
        //    }
        //}
        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var stats = new DashboardStatsDto();
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            try
            {
                stats.TotalRequests = await _context.PickupRequests.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting total pickup requests");
                stats.TotalRequests = 0;
            }

            try
            {
                stats.ActiveDrivers = await _context.DriverProfiles
                    .Where(d => d.IsAvailable)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting active drivers");
                stats.ActiveDrivers = 0;
            }

            try
            {
                stats.CompletedPickups = await _context.PickupRequests
                    .Where(r => r.Status == "Completed")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting completed pickups");
                stats.CompletedPickups = 0;
            }

            try
            {
                stats.PendingApprovals = await _context.PickupRequests
                    .Where(r => r.Status == "Pending")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting pending approvals");
                stats.PendingApprovals = 0;
            }

            try
            {
                stats.TodayPickups = await _context.PickupRequests
                    .Where(r => r.CreatedAt >= today && r.CreatedAt < tomorrow && r.Status == "Completed")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting today pickups");
                stats.TodayPickups = 0;
            }

            try
            {
                stats.RevenueThisMonth = await _context.PickupRequests
                    .Where(r => r.CreatedAt >= firstDayOfMonth && r.Status == "Completed")
                    .Select(r => (decimal?)r.TotalAmount ?? 0) // ✅ Safe for null
                    .SumAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating revenue this month");
                stats.RevenueThisMonth = 0;
            }

            _logger.LogInformation("✅ Dashboard statistics calculated: {Stats}",
                $"Total: {stats.TotalRequests}, Active: {stats.ActiveDrivers}, Today: {stats.TodayPickups}, Revenue: {stats.RevenueThisMonth}");

            return stats;
        }

        public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
        {
            try
            {
                _logger.LogInformation("📋 Fetching recent activities");

                var activities = new List<RecentActivityDto>();
                activities.AddRange(await GetRecentAssignmentsAsync());
                activities.AddRange(await GetRecentPickupsAsync());
                activities.AddRange(await GetRecentRequestsAsync());
                activities.AddRange(await GetRecentDriverRegistrationsAsync());

                var result = activities
                    .Where(a => a != null)
                    .OrderByDescending(a => a.Timestamp)
                    .Take(10)
                    .ToList();

                _logger.LogInformation($"✅ Retrieved {result.Count} recent activities");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving recent activities");
                return new List<RecentActivityDto>();
            }
        }

        // ----------- Private helpers with null-safety ------------

        private async Task<List<RecentActivityDto>> GetRecentAssignmentsAsync()
        {
            try
            {
                var assignments = await _context.DriverAssignments
                    .Include(a => a.Driver)
                    .ThenInclude(d => d.User)
                    .Where(a => a.IsActive && a.Status == AssignmentStatus.Assigned)
                    .OrderByDescending(a => a.AssignedAt)
                    .Take(5)
                    .Select(a => new
                    {
                        a.AssignmentId,
                        a.RequestId,
                        a.AssignedAt,
                        DriverFirstName = a.Driver != null && a.Driver.User != null ? a.Driver.User.FirstName : "Unknown",
                        DriverLastName = a.Driver != null && a.Driver.User != null ? a.Driver.User.LastName : "Driver"
                    })
                    .ToListAsync();

                return assignments.Select(a => new RecentActivityDto
                {
                    Id = a.AssignmentId.ToString(),
                    Type = "assignment",
                    Message = $"Driver {a.DriverFirstName} {a.DriverLastName} assigned to Request #{a.RequestId.ToString().Substring(0, Math.Min(8, a.RequestId.ToString().Length))}",
                    Timestamp = a.AssignedAt,
                    Icon = "🚛"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error fetching recent assignments");
                return new List<RecentActivityDto>();
            }
        }

        private async Task<List<RecentActivityDto>> GetRecentPickupsAsync()
        {
            try
            {
                var pickups = await _context.DriverAssignments
                    .Where(a => a.Status == AssignmentStatus.Completed)
                    .OrderByDescending(a => a.CompletedAt ?? a.AssignedAt)
                    .Take(5)
                    .Select(a => new
                    {
                        a.AssignmentId,
                        a.RequestId,
                        a.CompletedAt,
                        a.AssignedAt
                    })
                    .ToListAsync();

                return pickups.Select(a => new RecentActivityDto
                {
                    Id = a.AssignmentId.ToString(),
                    Type = "pickup",
                    Message = $"Pickup completed for Request #{a.RequestId.ToString().Substring(0, Math.Min(8, a.RequestId.ToString().Length))}",
                    Timestamp = a.CompletedAt ?? a.AssignedAt,
                    Icon = "✅"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error fetching recent pickups");
                return new List<RecentActivityDto>();
            }
        }

        private async Task<List<RecentActivityDto>> GetRecentRequestsAsync()
        {
            try
            {
                var requests = await _context.PickupRequests
                    .Where(r => r.Status == "Pending" || r.Status == "Approved")
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new
                    {
                        r.RequestId,
                        r.CreatedAt
                    })
                    .ToListAsync();

                return requests.Select(r => new RecentActivityDto
                {
                    Id = r.RequestId.ToString(),
                    Type = "approval",
                    Message = $"Request #{r.RequestId.ToString().Substring(0, Math.Min(8, r.RequestId.ToString().Length))} submitted",
                    Timestamp = r.CreatedAt,
                    Icon = "📝"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error fetching recent requests");
                return new List<RecentActivityDto>();
            }
        }

        private async Task<List<RecentActivityDto>> GetRecentDriverRegistrationsAsync()
        {
            try
            {
                var drivers = await _context.DriverProfiles
                    .Include(d => d.User)
                    .Where(d => d.User != null)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(5)
                    .Select(d => new
                    {
                        d.Id,
                        d.CreatedAt,
                        UserFirstName = d.User.FirstName ?? "Unknown",
                        UserLastName = d.User.LastName ?? "Driver"
                    })
                    .ToListAsync();

                return drivers.Select(d => new RecentActivityDto
                {
                    Id = d.Id.ToString(),
                    Type = "registration",
                    Message = $"New driver registration: {d.UserFirstName} {d.UserLastName}",
                    Timestamp = d.CreatedAt,
                    Icon = "👤"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error fetching recent driver registrations");
                return new List<RecentActivityDto>();
            }
        }
    }
}
