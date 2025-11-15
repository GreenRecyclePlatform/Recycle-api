using System.Collections.Generic;
using System.Threading.Tasks;
using recycle.Application.DTOs.Notifications;

namespace recycle.Application.Interfaces.IService
{
    public interface INotificationService
    {
        // Create/Send
        Task<NotificationDto> SendNotificationAsync(
            int userId,
            string notificationType,
            string title,
            string message,
            string relatedEntityType = null,
            int? relatedEntityId = null,
            string priority = "Normal");

        // Read
        Task<NotificationDto> GetNotificationByIdAsync(int userId, int notificationId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false);
        Task<int> GetUnreadCountAsync(int userId);
        Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId);

        // Update
        Task<bool> MarkAsReadAsync(int userId, int notificationId);
        Task<int> MarkMultipleAsReadAsync(int userId, int[] notificationIds);
        Task<int> MarkAllAsReadAsync(int userId);

        // Delete
        Task<bool> DeleteNotificationAsync(int userId, int notificationId);
        Task<int> DeleteAllNotificationsAsync(int userId);

        // Bulk operations
        Task SendBulkNotificationsAsync(int[] userIds, string notificationType, string title, string message);
    }
}