using Microsoft.AspNetCore.SignalR;
using recycle.Application.DTOs.Notifications;
using recycle.Application.Interfaces;
using recycle.Infrastructure.Hubs;

namespace recycle.Infrastructure.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationHubService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(Guid userId, NotificationDto notification)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task NotifyNotificationMarkedAsReadAsync(Guid userId, Guid notificationId)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationMarkedAsRead", notificationId);
        }

        public async Task NotifyNotificationBatchReadAsync(Guid userId, Guid[] notificationIds)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationBatchRead", notificationIds);
        }

        public async Task NotifyAllNotificationsReadAsync(Guid userId)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationAllRead");
        }

        public async Task NotifyNotificationDeletedAsync(Guid userId, Guid notificationId)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationDeleted", notificationId);
        }

        public async Task NotifyAllNotificationsDeletedAsync(Guid userId)
        {
            await _hubContext.Clients.Group($"User_{userId}")
                .SendAsync("NotificationAllDeleted");
        }
    }
}