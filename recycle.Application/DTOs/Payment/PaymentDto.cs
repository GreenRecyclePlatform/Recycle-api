using System;

namespace recycle.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid ID { get; set; } // CHANGED: Match entity property name
        public Guid RequestId { get; set; }
        public Guid RecipientUserID { get; set; } // CHANGED: Match entity property name
        public string RecipientType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }
        public string PaymentStatus { get; set; } = string.Empty; // CHANGED: Remove nullable

        // ADDED: Additional fields for tracking
        public Guid? ApprovedByAdminID { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? FailedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? AdminNotes { get; set; }
        public string? FailureReason { get; set; }
    }
}