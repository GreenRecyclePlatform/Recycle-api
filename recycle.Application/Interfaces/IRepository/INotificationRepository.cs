using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{

    public interface INotificationRepository : IRepository<Notification>
    {


        Task<int> GetUnreadCountAsync(Guid userId);


        Task<bool> MarkAsReadAsync(Guid notificationId);


        Task<int> MarkAllAsReadAsync(Guid userId);


        Task<List<Notification>> GetNotificationsByTypeAsync(
            Guid userId,
            string notificationType);


        Task<int> DeleteOldReadNotificationsAsync(int daysOld = 90);

    }
}