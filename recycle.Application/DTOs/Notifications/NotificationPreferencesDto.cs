using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public class NotificationPreferencesDto
    {
        public Guid UserId { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool PushNotifications { get; set; }

        public bool ReceiveDriverAssignments { get; set; }
        public bool ReceivePaymentUpdates { get; set; }
        public bool ReceiveReviewNotifications { get; set; }
        public bool ReceiveSystemAnnouncements { get; set; }
    }
}
