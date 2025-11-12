using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationFilterDto
    {
        public bool? UnreadOnly { get; set; }
        public string NotificationType { get; set; }
        public string Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
