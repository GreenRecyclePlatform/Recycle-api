namespace recycle.Domain.Entities
{
    /// <summary>
    /// Constants for notification types used throughout the application
    /// </summary>
    public static class NotificationTypes
    {
        // ==================== USER NOTIFICATIONS ====================
        /// <summary>
        /// Notification sent when user creates a new pickup request
        /// </summary>
        public const string RequestCreated = "RequestCreated";

        /// <summary>
        /// Notification sent when admin assigns a driver to user's request
        /// </summary>
        public const string DriverAssigned = "DriverAssigned";

        /// <summary>
        /// Notification sent when driver is on the way to pickup location
        /// </summary>
        public const string DriverEnRoute = "DriverEnRoute";

        /// <summary>
        /// Notification sent when driver completes the pickup
        /// </summary>
        public const string PickupCompleted = "PickupCompleted";

        /// <summary>
        /// Notification sent when admin approves user's payment
        /// </summary>
        public const string PaymentApproved = "PaymentApproved";

        /// <summary>
        /// Notification sent when payment has been successfully transferred to user
        /// </summary>
        public const string PaymentPaid = "PaymentPaid";

        /// <summary>
        /// Reminder notification sent to user to leave a review for completed pickup
        /// </summary>
        public const string ReviewReminder = "ReviewReminder";

        /// <summary>
        /// Notification sent when user's request is cancelled
        /// </summary>
        public const string RequestCancelled = "RequestCancelled";

        // ==================== DRIVER NOTIFICATIONS ====================
        /// <summary>
        /// Notification sent when admin assigns a new pickup request to driver
        /// </summary>
        public const string NewAssignment = "NewAssignment";

        /// <summary>
        /// Notification sent when driver's assignment is cancelled by admin
        /// </summary>
        public const string AssignmentCancelled = "AssignmentCancelled";

        /// <summary>
        /// Notification sent when admin approves driver's payment
        /// </summary>
        public const string DriverPaymentApproved = "DriverPaymentApproved";

        /// <summary>
        /// Notification sent when driver's payment has been transferred
        /// </summary>
        public const string DriverPaymentPaid = "DriverPaymentPaid";

        /// <summary>
        /// Notification sent when driver receives a new review from user
        /// </summary>
        public const string NewReview = "NewReview";

        /// <summary>
        /// Notification sent when user cancels a pickup request assigned to driver
        /// </summary>
        public const string UserCancelledRequest = "UserCancelledRequest";

        // ==================== ADMIN NOTIFICATIONS ====================
        /// <summary>
        /// Notification sent when a new pickup request is created and pending assignment
        /// </summary>
        public const string NewRequestPending = "NewRequestPending";

        /// <summary>
        /// Notification sent when a pickup is completed and ready for admin review/approval
        /// </summary>
        public const string PickupReadyForReview = "PickupReadyForReview";

        /// <summary>
        /// Notification sent when driver rejects an assignment
        /// </summary>
        public const string DriverRejectedAssignment = "DriverRejectedAssignment";

        /// <summary>
        /// Notification sent when a payment fails to process
        /// </summary>
        public const string PaymentFailed = "PaymentFailed";

        /// <summary>
        /// Notification sent when a review is flagged for inappropriate content
        /// </summary>
        public const string ReviewFlagged = "ReviewFlagged";

        // ==================== SYSTEM NOTIFICATIONS ====================
        /// <summary>
        /// System-wide announcement notification
        /// </summary>
        public const string SystemAnnouncement = "SystemAnnouncement";

        /// <summary>
        /// Maintenance notification
        /// </summary>
        public const string SystemMaintenance = "SystemMaintenance";

        /// <summary>
        /// Welcome notification for new users
        /// </summary>
        public const string Welcome = "Welcome";
    }

    /// <summary>
    /// Constants for notification priorities
    /// </summary>
    public static class NotificationPriorities
    {
        public const string Low = "Low";
        public const string Normal = "Normal";
        public const string High = "High";
        public const string Urgent = "Urgent";
    }

    /// <summary>
    /// Constants for related entity types in notifications
    /// </summary>
    public static class NotificationEntityTypes
    {
        public const string PickupRequest = "PickupRequest";
        public const string Payment = "Payment";
        public const string Review = "Review";
        public const string DriverAssignment = "DriverAssignment";
        public const string User = "User";
    }
}