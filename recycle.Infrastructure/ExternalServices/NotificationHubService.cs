using Microsoft.AspNetCore.SignalR;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using recycle.Infrastructure.Hubs;
using recycle.Infrastructure.Repositories;
using System.Data;

namespace recycle.Infrastructure.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationHubService(IHubContext<NotificationHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        public async Task SendToUser(Guid userId, NotificationDto notificationDto)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                NotificationType = notificationDto.Type,
                RelatedEntityType = notificationDto.RelatedEntityType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save notification to the database
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();


            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);


            // Update unread count
            var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateUnreadCount", unreadCount);
        }

        public async Task SendPendingNotification(Guid userId)
        {
            var pendingNotification = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && !n.IsRead);

            foreach (var notification in pendingNotification)
            {
                await _hubContext.Clients.User(userId.ToString())
                .SendAsync("ReceiveNotification", notification);
            }
            await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveChangesAsync();
        }

        //public async Task NotifyRead(Guid userId, Guid notificationId)
        //{
        //    await _hubContext.Clients.Group($"User_{userId}")
        //        .SendAsync("NotificationMarkedAsRead", notificationId);
        //}
        public async Task SendToRole(string role,NotificationDto notificationDto)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                NotificationType = notificationDto.Type,
                RelatedEntityType = notificationDto.RelatedEntityType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                 UserId = null
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification",notification);
        }
    }
}