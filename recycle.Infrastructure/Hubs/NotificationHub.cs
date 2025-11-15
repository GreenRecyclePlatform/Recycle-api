using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace recycle.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("sub")?.Value
                      ?? Context.User?.FindFirst("userId")?.Value
                      ?? Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst("sub")?.Value
                      ?? Context.User?.FindFirst("userId")?.Value
                      ?? Context.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}