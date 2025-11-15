using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using recycle.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace recycle.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationHubService _notificationHubService;

        public NotificationHub(INotificationRepository notificationRepository,INotificationHubService notificationHubService)
        {
            _notificationRepository = notificationRepository;
            _notificationHubService = notificationHubService;
        }
       
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;




            if (!string.IsNullOrEmpty(userId))
            {
                var isAdmin = Context.User?.IsInRole("Admin") ?? false;
                if (isAdmin)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
                }
                else
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                }
                    await _notificationHubService.SendPendingNotification(Guid.Parse(userId));

                var count = await _notificationRepository.GetUnreadCountAsync(Guid.Parse(userId));
                await Clients.Caller.SendAsync("ReceiveUnreadCount", count);

               
            }

            await base.OnConnectedAsync();
        }

        //private async Task SendPendingNotifications(Guid userId)
        //{
        //    var pendingNotifications = await _notificationRepository.GetAll(
        //        filter: n => n.UserId == userId && !n.IsRead);

        //    // Here you would typically send the pending notifications to the user.

        //    foreach (var notification in pendingNotifications)
        //    {
        //        await Clients.Group($"User_{userId}")
        //                .SendAsync("ReceivePendingNotification", notification);
        //    };
        //}

        public override async Task OnDisconnectedAsync(Exception exception)
        {
                      
            await base.OnDisconnectedAsync(exception);
        }
    }
}