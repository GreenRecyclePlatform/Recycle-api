using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationTypeStats
    {
        public string NotificationType { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
    }
}
