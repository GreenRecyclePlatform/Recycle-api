using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationStatisticsDto
    {
        public int TotalNotifications { get; set; }
        public int TotalUnread { get; set; }
        public int TotalRead { get; set; }
        public double ReadRate { get; set; }

        public NotificationTypeStats[] ByType { get; set; }
        public NotificationPriorityStats[] ByPriority { get; set; }
    }
}
