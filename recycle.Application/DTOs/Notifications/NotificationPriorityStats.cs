using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationPriorityStats
    {
        public string Priority { get; set; }
        public int Count { get; set; }
        public int UnreadCount { get; set; }
    }
}
