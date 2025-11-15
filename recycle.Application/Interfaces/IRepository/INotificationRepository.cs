using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    /// <summary>
    /// Specific repository interface for Notification entity
    /// Extends the generic IRepository with notification-specific methods
    /// </summary>
    public interface INotificationRepository : IRepository<Notification>
    {
        /// <summary>
        /// Get all notifications for a specific user
        /// </summary>
        Task<List<Notification>> GetUserNotificationsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false);

        /// <summary>
        /// Get unread notification count for a user
        /// </summary>
        Task<int> GetUnreadCountAsync(Guid userId);

        /// <summary>
        /// Get the latest notification for a user
        /// </summary>
        Task<Notification> GetLatestNotificationAsync(Guid userId);

        /// <summary>
        /// Mark a notification as read
        /// </summary>
        Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        Task<int> MarkMultipleAsReadAsync(Guid[] notificationIds, Guid userId);

        /// <summary>
        /// Mark all user's notifications as read
        /// </summary>
        Task<int> MarkAllAsReadAsync(Guid userId);

        /// <summary>
        /// Delete all notifications for a user
        /// </summary>
        Task<int> DeleteUserNotificationsAsync(Guid userId);

        /// <summary>
        /// Get notifications by type for a user
        /// </summary>
        Task<List<Notification>> GetNotificationsByTypeAsync(
            Guid userId,
            string notificationType);

        /// <summary>
        /// Get notifications by priority
        /// </summary>
        Task<List<Notification>> GetNotificationsByPriorityAsync(
            Guid userId,
            string priority);

        /// <summary>
        /// Delete old read notifications (cleanup)
        /// </summary>
        Task<int> DeleteOldReadNotificationsAsync(int daysOld = 90);

        /// <summary>
        /// Check if a notification belongs to a user
        /// </summary>
        Task<bool> NotificationBelongsToUserAsync(Guid notificationId, Guid userId);
    }
}