using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.DTOs.Notifications;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(userIdClaim);
        }

        /// <summary>
        /// Get current user's notifications (paginated)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(NotificationListResponse), 200)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool unreadOnly = false)
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(
                userId, page, pageSize, unreadOnly);

            return Ok(new NotificationListResponse
            {
                Success = true,
                Data = notifications,
                Page = page,
                PageSize = pageSize,
                TotalCount = notifications.Count
            });
        }

        /// <summary>
        /// Get unread notification count for current user
        /// </summary>
        [HttpGet("unread-count")]
        [ProducesResponseType(typeof(UnreadCountResponse), 200)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new UnreadCountResponse
            {
                Success = true,
                UnreadCount = count
            });
        }

        /// <summary>
        /// Get notification summary (unread count + latest notification)
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NotificationSummaryDto>), 200)]
        public async Task<IActionResult> GetSummary()
        {
            var userId = GetCurrentUserId();
            var summary = await _notificationService.GetNotificationSummaryAsync(userId);

            return Ok(new ApiResponse<NotificationSummaryDto>
            {
                Success = true,
                Data = summary
            });
        }

        /// <summary>
        /// Mark a single notification as read
        /// </summary>
        [HttpPut("{notificationId}/read")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.MarkAsReadAsync(userId, notificationId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Notification not found or doesn't belong to you"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Notification marked as read"
            });
        }

        /// <summary>
        /// Mark multiple notifications as read
        /// </summary>
        [HttpPut("mark-multiple-read")]
        [ProducesResponseType(typeof(MarkMultipleResponse), 200)]
        public async Task<IActionResult> MarkMultipleAsRead([FromBody] MarkAsReadDto dto)
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkMultipleAsReadAsync(userId, dto.NotificationIds);

            return Ok(new MarkMultipleResponse
            {
                Success = true,
                MarkedCount = count,
                Message = $"{count} notification(s) marked as read"
            });
        }

        /// <summary>
        /// Mark all user's notifications as read
        /// </summary>
        [HttpPut("read-all")]
        [ProducesResponseType(typeof(MarkMultipleResponse), 200)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkAllAsReadAsync(userId);

            return Ok(new MarkMultipleResponse
            {
                Success = true,
                MarkedCount = count,
                Message = $"All notifications marked as read ({count} total)"
            });
        }

        /// <summary>
        /// Delete a notification
        /// </summary>
        [HttpDelete("{notificationId}")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> DeleteNotification(Guid notificationId)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.DeleteNotificationAsync(userId, notificationId);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Notification not found"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Notification deleted"
            });
        }

        /// <summary>
        /// Delete all user's notifications
        /// </summary>
        [HttpDelete("delete-all")]
        [ProducesResponseType(typeof(MarkMultipleResponse), 200)]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.DeleteAllNotificationsAsync(userId);

            return Ok(new MarkMultipleResponse
            {
                Success = true,
                MarkedCount = count,
                Message = $"{count} notification(s) deleted"
            });
        }

        /// <summary>
        /// Send notification (Admin/System only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.SendNotificationAsync(
                dto.UserId,
                dto.NotificationType,
                dto.Title,
                dto.Message,
                dto.RelatedEntityType,
                dto.RelatedEntityId,
                dto.Priority
            );

            return Ok(new ApiResponse<NotificationDto>
            {
                Success = true,
                Data = notification,
                Message = "Notification sent successfully"
            });
        }

        /// <summary>
        /// Get notification by ID
        /// </summary>
        [HttpGet("{notificationId}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationDto>), 200)]
        public async Task<IActionResult> GetNotificationById(Guid notificationId)
        {
            var userId = GetCurrentUserId();
            var notification = await _notificationService.GetNotificationByIdAsync(userId, notificationId);

            if (notification == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Notification not found"
                });

            return Ok(new ApiResponse<NotificationDto>
            {
                Success = true,
                Data = notification
            });
        }

        /// <summary>
        /// Send bulk notification to multiple users (Admin only)
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), 200)]
        public async Task<IActionResult> SendBulkNotification([FromBody] BulkNotificationDto dto)
        {
            await _notificationService.SendBulkNotificationsAsync(
                dto.UserIds,
                dto.NotificationType,
                dto.Title,
                dto.Message
            );

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Notification sent to {dto.UserIds.Length} user(s)"
            });
        }
    }

    // Response Models
   

    public class NotificationListResponse
    {
        public bool Success { get; set; }
        public List<NotificationDto> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }

    public class UnreadCountResponse
    {
        public bool Success { get; set; }
        public int UnreadCount { get; set; }
    }

    public class MarkMultipleResponse
    {
        public bool Success { get; set; }
        public int MarkedCount { get; set; }
        public string Message { get; set; }
    }
}