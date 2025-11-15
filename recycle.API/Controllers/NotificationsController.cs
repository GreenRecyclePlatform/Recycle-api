using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using recycle.Application.Interfaces;

namespace recycle.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {

        private readonly INotificationHubService notificationHubService;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationsController(INotificationHubService notficationHubService,IUnitOfWork unitOfWork)
        {

            this.notificationHubService = notficationHubService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("send-to-user/{userId}")]
        public async Task<IActionResult> SendToUser(Guid userId, [FromBody] NotificationDto notification)
        {
            await notificationHubService.SendToUser(userId, notification);
            return Ok("Notification sent");
        }

        [HttpPost("send-to-admins")]
        public async Task<IActionResult> SendToAdmins([FromBody] NotificationDto notification)
        {
            await notificationHubService.SendToRole("Admin", notification);
            return Ok("Notification sent to admins");
        }

        [HttpGet("getAllNotification")]
        public async Task<IActionResult> GetAllNotification()
        {
            var userId = GetCurrentUserId(); 

            var notifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId,
                orderBy: q => q.OrderByDescending(n => n.CreatedAt)
            );

            return Ok(notifications);
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            var count = await _unitOfWork.Notifications.MarkAllAsReadAsync(userId);

            return Ok();           
        }

        [HttpGet("unread-notification")]
        public async Task<IActionResult> GetUnreadNotification()
        {
            var userId = GetCurrentUserId();
            var notifications = await _unitOfWork.Notifications.GetAll(
                filter: n => n.UserId == userId && !n.IsRead,
                orderBy: q => q.OrderByDescending(n => n.CreatedAt)
            );
            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _unitOfWork.Notifications.GetUnreadCountAsync(userId);
            return Ok(new { UnreadCount = count });
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                throw new UnauthorizedAccessException("User ID not found in token");

            return Guid.Parse(userIdClaim);
        }

      
    }
}