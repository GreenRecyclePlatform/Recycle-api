using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace recycle.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var userRoles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            Console.WriteLine($"✅ User {userId} connected to NotificationHub");
            Console.WriteLine($"👤 User roles: {string.Join(", ", userRoles ?? new List<string>())}");

            // Add user to role-based groups
            if (userRoles != null)
            {
                foreach (var role in userRoles)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, role);
                    Console.WriteLine($"➕ Added user {userId} to group: {role}");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var userRoles = Context.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            Console.WriteLine($"❌ User {userId} disconnected from NotificationHub");

            // Remove user from role-based groups
            if (userRoles != null)
            {
                foreach (var role in userRoles)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, role);
                    Console.WriteLine($"➖ Removed user {userId} from group: {role}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Optional: Method to manually join a group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"➕ User {Context.UserIdentifier} joined group: {groupName}");
        }

        // Optional: Method to manually leave a group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"➖ User {Context.UserIdentifier} left group: {groupName}");
        }

        // Optional: Test method to verify connection
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
            Console.WriteLine($"🏓 Ping received from user {Context.UserIdentifier}");
        }
    }
}