using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Repositories
{
    /// <summary>
    /// Specific repository implementation for Notification entity
    /// Inherits from generic Repository and implements INotificationRepository
    /// </summary>
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification> GetLatestNotificationAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkMultipleAsReadAsync(Guid[] notificationIds, Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && notificationIds.Contains(n.NotificationId))
                .ToListAsync();

            var count = 0;
            foreach (var notification in notifications)
            {
                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    count++;
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            var count = unreadNotifications.Count;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return count;
        }

        public async Task<int> DeleteUserNotificationsAsync(Guid userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            var count = notifications.Count;

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return count;
        }

        public async Task<List<Notification>> GetNotificationsByTypeAsync(
            Guid userId,
            string notificationType)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.NotificationType == notificationType)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetNotificationsByPriorityAsync(
            Guid userId,
            string priority)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && n.Priority == priority)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> DeleteOldReadNotificationsAsync(int daysOld = 90)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);

            var oldNotifications = await _context.Notifications
                .Where(n => n.IsRead && n.CreatedAt < cutoffDate)
                .ToListAsync();

            var count = oldNotifications.Count;

            _context.Notifications.RemoveRange(oldNotifications);
            await _context.SaveChangesAsync();

            return count;
        }

        public async Task<bool> NotificationBelongsToUserAsync(Guid notificationId, Guid userId)
        {
            return await _context.Notifications
                .AnyAsync(n => n.NotificationId == notificationId && n.UserId == userId);
        }
    }
}