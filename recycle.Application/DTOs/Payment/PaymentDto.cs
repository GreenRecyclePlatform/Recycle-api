using System;

namespace recycle.Application.DTOs.Payment
{
    public class PaymentDto
    {
        //public Guid ID { get; set; } // CHANGED: Match entity property name
        //public Guid RequestId { get; set; }
        //public Guid RecipientUserID { get; set; } // CHANGED: Match entity property name
        //public string RecipientType { get; set; } = string.Empty;
        //public decimal Amount { get; set; }
        //public string PaymentMethod { get; set; } = string.Empty;
        //public string? TransactionReference { get; set; }
        //public string PaymentStatus { get; set; } = string.Empty; // CHANGED: Remove nullable

        //// ADDED: Additional fields for tracking
        //public Guid? ApprovedByAdminID { get; set; }
        //public DateTime? ApprovedAt { get; set; }
        //public DateTime? PaidAt { get; set; }
        //public DateTime? FailedAt { get; set; }
        //public DateTime CreatedAt { get; set; }

        //public string? AdminNotes { get; set; }
        //public string? FailureReason { get; set; }

        // ✅ CHANGED: ID → paymentId (to match frontend)
        public Guid paymentId { get; set; }

        // ✅ CHANGED: RecipientUserID → userId (to match frontend)
        public Guid userId { get; set; }

        public string userName { get; set; } = string.Empty;

        // ✅ CHANGED: RequestId → pickupRequestId (to match frontend)
        public Guid? pickupRequestId { get; set; }

        public decimal amount { get; set; }

        // ✅ NEW: type field (frontend expects this)
        public string type { get; set; } = "Earning";  // Default to Earning

        // ✅ CHANGED: PaymentStatus → status (to match frontend)
        public string status { get; set; } = string.Empty;

        // ✅ CHANGED: PaymentMethod → paymentMethod (to match frontend)
        public string? paymentMethod { get; set; }

        // ✅ CHANGED: TransactionReference → transactionId (to match frontend)
        public string? transactionId { get; set; }

        public string description { get; set; } = string.Empty;

        public DateTime createdAt { get; set; }

        // ✅ NEW: processedAt (frontend expects this)
        public DateTime? processedAt { get; set; }

        // ✅ NEW: completedAt (frontend expects this)
        public DateTime? completedAt { get; set; }

        // ✅ CHANGED: FailureReason → failureReason (to match frontend)
        public string? failureReason { get; set; }


    }
}