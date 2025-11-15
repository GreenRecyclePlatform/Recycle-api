using recycle.Application.DTOs.Notifications;

namespace recycle.Application.Interfaces
{
    public interface INotificationHubService
    {
        Task SendNotificationToUserAsync(Guid userId, NotificationDto notification);
        Task NotifyNotificationMarkedAsReadAsync(Guid userId, Guid notificationId);
        Task NotifyNotificationBatchReadAsync(Guid userId, Guid[] notificationIds);
        Task NotifyAllNotificationsReadAsync(Guid userId);
        Task NotifyNotificationDeletedAsync(Guid userId, Guid notificationId);
        Task NotifyAllNotificationsDeletedAsync(Guid userId);
    }
}