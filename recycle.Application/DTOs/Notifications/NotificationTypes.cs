using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Notifications
{
    public static class NotificationTypes
    {
        public const string RequestCreated = "RequestCreated";
        public const string RequestApproved = "RequestApproved";
        public const string RequestRejected = "RequestRejected";
        public const string RequestAssigned = "RequestAssigned";
        public const string RequestCancelled = "RequestCancelled";
        public const string NewRequestPending = "NewRequestPending";
        public const string NewAssignment = "NewAssignment";
        public const string PickupCompleted = "PickupCompleted";
    }
}
