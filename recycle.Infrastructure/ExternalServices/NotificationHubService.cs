using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using recycle.Infrastructure.Hubs;

namespace recycle.Infrastructure.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationHubService> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHubService(
            IHubContext<NotificationHub> hubContext,
            IUnitOfWork unitOfWork,
            ILogger<NotificationHubService> logger,
            UserManager<ApplicationUser> userManager)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task SendToUser(Guid userId, NotificationDto notificationDto)
        {
            _logger.LogInformation("📤 Sending notification to user {UserId}", userId);
            _logger.LogInformation("📝 Title: {Title}", notificationDto.Title);
            _logger.LogInformation("📝 Type: {Type}", notificationDto.Type);

            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = userId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                NotificationType = notificationDto.Type,
                RelatedEntityType = notificationDto.RelatedEntityType,
                RelatedEntityId = notificationDto.RelatedEntityId,
                Priority = notificationDto.Priority ?? "Normal",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save notification to the database
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("💾 Notification saved to database with ID: {NotificationId}", notification.NotificationId);

            // Send notification via SignalR
            try
            {
                await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("✅ SignalR notification sent to user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send SignalR notification to user {UserId}", userId);
            }

            // Get and send updated unread count
            try
            {
                var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
                await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateUnreadCount", unreadCount);
                _logger.LogInformation("📊 Unread count ({Count}) sent to user {UserId}", unreadCount, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send unread count to user {UserId}", userId);
            }
        }

        public async Task SendPendingNotification(Guid userId)
        {
            _logger.LogInformation("📬 Sending pending notifications to user {UserId}", userId);

            // Get all unread notifications for the user
            var pendingNotifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && !n.IsRead);

            var notificationsList = pendingNotifications.ToList();
            _logger.LogInformation("📋 Found {Count} pending notifications", notificationsList.Count);

            // Send each notification via SignalR
            foreach (var notification in notificationsList)
            {
                try
                {
                    await _hubContext.Clients.User(userId.ToString())
                        .SendAsync("ReceiveNotification", notification);
                    _logger.LogInformation("✅ Sent pending notification {NotificationId}", notification.NotificationId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send pending notification {NotificationId}", notification.NotificationId);
                }
            }

            // Send updated unread count
            try
            {
                var unreadCount = notificationsList.Count;
                await _hubContext.Clients.User(userId.ToString()).SendAsync("UpdateUnreadCount", unreadCount);
                _logger.LogInformation("📊 Pending unread count ({Count}) sent to user {UserId}", unreadCount, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send pending unread count to user {UserId}", userId);
            }
        }

        public async Task SendToRole(string role, NotificationDto notificationDto)
        {
            _logger.LogInformation("📤 Sending notification to role: {Role}", role);
            _logger.LogInformation("📝 Title: {Title}", notificationDto.Title);
            _logger.LogInformation("📝 Type: {Type}", notificationDto.Type);

            // ✅ Get all users with this role using UserManager
            var usersInRole = await _userManager.GetUsersInRoleAsync(role);
            var usersList = usersInRole.ToList();

            _logger.LogInformation("👥 Found {Count} users in role {Role}", usersList.Count, role);

            if (usersList.Count == 0)
            {
                _logger.LogWarning("⚠️ No users found in role {Role}. Notification will not be sent.", role);
                return;
            }

            // Create and save notification for each user
            foreach (var user in usersList)
            {
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = user.Id,
                    Title = notificationDto.Title,
                    Message = notificationDto.Message,
                    NotificationType = notificationDto.Type,
                    RelatedEntityType = notificationDto.RelatedEntityType,
                    RelatedEntityId = notificationDto.RelatedEntityId,
                    Priority = notificationDto.Priority ?? "Normal",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                _logger.LogInformation("💾 Notification created for user {UserId} ({UserName}) in role {Role}",
                    user.Id, user.UserName, role);
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("💾 All {Count} notifications saved to database", usersList.Count);

            // Send to SignalR group
            try
            {
                var signalRPayload = new
                {
                    NotificationId = Guid.NewGuid(), // Temporary ID for SignalR
                    Title = notificationDto.Title,
                    Message = notificationDto.Message,
                    NotificationType = notificationDto.Type,
                    Type = notificationDto.Type, // Add both for compatibility
                    RelatedEntityType = notificationDto.RelatedEntityType,
                    RelatedEntityId = notificationDto.RelatedEntityId,
                    Priority = notificationDto.Priority ?? "Normal",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", signalRPayload);
                _logger.LogInformation("✅ SignalR notification sent to group {Role}", role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to send SignalR notification to group {Role}", role);
            }

            // Send individual unread count updates to each user
            foreach (var user in usersList)
            {
                try
                {
                    var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(user.Id);
                    await _hubContext.Clients.User(user.Id.ToString()).SendAsync("UpdateUnreadCount", unreadCount);
                    _logger.LogInformation("📊 Unread count ({Count}) sent to user {UserId} ({UserName})",
                        unreadCount, user.Id, user.UserName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to send unread count to user {UserId}", user.Id);
                }
            }
        }
    }
}