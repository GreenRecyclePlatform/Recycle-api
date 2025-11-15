using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using recycle.Application.DTOs.Notifications;

namespace recycle.Application.Interfaces
{
    public interface INotificationService
    {
        // Create/Send (using Guid for userId)
        Task<NotificationDto> SendNotificationAsync(
            Guid userId,
            string notificationType,
            string title,
            string message,
            string relatedEntityType = null,
            Guid? relatedEntityId = null,
            string priority = "Normal");

        // Read (using Guid)
        Task<NotificationDto> GetNotificationByIdAsync(Guid userId, Guid notificationId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId);

        // Update (using Guid)
        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId);
        Task<int> MarkMultipleAsReadAsync(Guid userId, Guid[] notificationIds);
        Task<int> MarkAllAsReadAsync(Guid userId);

        // Delete (using Guid)
        Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId);
        Task<int> DeleteAllNotificationsAsync(Guid userId);

        // Bulk operations (using Guid)
        Task SendBulkNotificationsAsync(Guid[] userIds, string notificationType, string title, string message);
    }
}