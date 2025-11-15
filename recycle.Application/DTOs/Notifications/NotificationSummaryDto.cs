using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationSummaryDto
    {
        public int UnreadCount { get; set; }
        public int TotalCount { get; set; }
        public NotificationDto LatestNotification { get; set; }
    }
}
