using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using recycle.Application.DTOs.Notifications;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;

namespace recycle.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NotificationDto> SendNotificationAsync(
            int userId,
            string notificationType,
            string title,
            string message,
            string relatedEntityType = null,
            int? relatedEntityId = null,
            string priority = "Normal")
        {
            var notification = new Notification
            {
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

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(notification);
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int userId, int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetAsync(
                filter: n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return null;

            return MapToDto(notification);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            int page = 1,
            int pageSize = 20,
            bool unreadOnly = false)
        {
            var notifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && (!unreadOnly || !n.IsRead),
                orderBy: q => q.OrderByDescending(n => n.CreatedAt),
                pageSize: pageSize,
                pageNumber: page);

            return notifications.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _unitOfWork.Notifications.CountAsync(
                filter: n => n.UserId == userId && !n.IsRead);
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(int userId)
        {
            var unreadCount = await GetUnreadCountAsync(userId);
            var totalCount = await _unitOfWork.Notifications.CountAsync(
                filter: n => n.UserId == userId);

            var latestNotifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId,
                orderBy: q => q.OrderByDescending(n => n.CreatedAt),
                pageSize: 1,
                pageNumber: 1);

            var latestNotification = latestNotifications.FirstOrDefault();

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                TotalCount = totalCount,
                LatestNotification = latestNotification != null
                    ? MapToDto(latestNotification)
                    : null
            };
        }

        public async Task<bool> MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetAsync(
                filter: n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            await _unitOfWork.Notifications.UpdateAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> MarkMultipleAsReadAsync(int userId, int[] notificationIds)
        {
            var notifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && notificationIds.Contains(n.NotificationId));

            var count = 0;
            foreach (var notification in notifications)
            {
                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    await _unitOfWork.Notifications.UpdateAsync(notification);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unreadNotifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && !n.IsRead);

            var count = 0;
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification);
                count++;
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task<bool> DeleteNotificationAsync(int userId, int notificationId)
        {
            var notification = await _unitOfWork.Notifications.GetAsync(
                filter: n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            await _unitOfWork.Notifications.RemoveAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<int> DeleteAllNotificationsAsync(int userId)
        {
            var notifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId);

            var count = notifications.Count();
            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.RemoveAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync();
            return count;
        }

        public async Task SendBulkNotificationsAsync(
            int[] userIds,
            string notificationType,
            string title,
            string message)
        {
            foreach (var userId in userIds)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    NotificationType = notificationType,
                    Title = title,
                    Message = message,
                    Priority = "Normal",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification);
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
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes > 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) > 1 ? "s" : "")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")} ago";
        }
    }
}