using recycle.Application.DTOs.Notifications;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;

namespace recycle.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationHubService _hubService;  // <-- Use interface instead

        public NotificationService(
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork,
            INotificationHubService hubService)
        {
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
            _hubService = hubService;
        }

        public async Task<NotificationDto> SendNotificationAsync(
            Guid userId,
            string notificationType,
            string title,
            string message,
            string relatedEntityType = null,
            Guid? relatedEntityId = null,
            string priority = "Normal")
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                NotificationType = notificationType,
                Title = title,
                Message = message,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId,
                Priority = priority,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            var dto = MapToDto(notification);

            // 🔥 Send via real-time SignalR through interface
            await _hubService.SendNotificationToUserAsync(userId, dto);

            return dto;
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(Guid userId, Guid notificationId)
        {
            var belongsToUser = await _notificationRepository.NotificationBelongsToUserAsync(
                notificationId, userId);

            if (!belongsToUser)
                return null;

            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            return notification != null ? MapToDto(notification) : null;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(
            Guid userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false)
        {
            var notifications = await _notificationRepository.GetUserNotificationsAsync(
                userId, page, pageSize, unreadOnly);

            return notifications.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(Guid userId)
        {
            var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId);

            var allNotifications = await _notificationRepository.GetAll(n => n.UserId == userId);
            var totalCount = allNotifications.Count();

            var latestNotification = await _notificationRepository.GetLatestNotificationAsync(userId);

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                TotalCount = totalCount,
                LatestNotification = latestNotification != null
                    ? MapToDto(latestNotification)
                    : null
            };
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId)
        {
            var result = await _notificationRepository.MarkAsReadAsync(notificationId, userId);

            if (result)
            {
                await _hubService.NotifyNotificationMarkedAsReadAsync(userId, notificationId);
            }

            return result;
        }

        public async Task<int> MarkMultipleAsReadAsync(Guid userId, Guid[] notificationIds)
        {
            var count = await _notificationRepository.MarkMultipleAsReadAsync(notificationIds, userId);

            if (count > 0)
            {
                await _hubService.NotifyNotificationBatchReadAsync(userId, notificationIds);
            }

            return count;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId)
        {
            var count = await _notificationRepository.MarkAllAsReadAsync(userId);

            if (count > 0)
            {
                await _hubService.NotifyAllNotificationsReadAsync(userId);
            }

            return count;
        }

        public async Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            var belongsToUser = await _notificationRepository.NotificationBelongsToUserAsync(
                notificationId, userId);

            if (!belongsToUser)
                return false;

            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
                return false;

            await _notificationRepository.RemoveAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubService.NotifyNotificationDeletedAsync(userId, notificationId);

            return true;
        }

        public async Task<int> DeleteAllNotificationsAsync(Guid userId)
        {
            var count = await _notificationRepository.DeleteUserNotificationsAsync(userId);

            if (count > 0)
            {
                await _hubService.NotifyAllNotificationsDeletedAsync(userId);
            }

            return count;
        }

        public async Task SendBulkNotificationsAsync(
            Guid[] userIds,
            string notificationType,
            string title,
            string message)
        {
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = userId,
                    NotificationType = notificationType,
                    Title = title,
                    Message = message,
                    Priority = "Normal",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);

                // 🔥 Real-time broadcasting
                await _hubService.SendNotificationToUserAsync(userId, MapToDto(notification));
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                NotificationType = notification.NotificationType,
                Title = notification.Title,
                Message = notification.Message,
                RelatedEntityType = notification.RelatedEntityType,
                RelatedEntityId = notification.RelatedEntityId,
                IsRead = notification.IsRead,
                Priority = notification.Priority,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                TimeAgo = GetTimeAgo(notification.CreatedAt)
            };
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hours ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} days ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} weeks ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} months ago";

            return $"{(int)(timeSpan.TotalDays / 365)} years ago";
        }
    }
}