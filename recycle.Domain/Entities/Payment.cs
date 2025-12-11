using recycle.Domain.Enums;

namespace recycle.Domain.Entities
{
    public class Payment
    {
        public Guid ID { get; set; } = Guid.NewGuid();

        public Guid RequestId { get; set; }

        public Guid RecipientUserID { get; set; }

        public string RecipientType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = PaymentStatuses.Pending;

        public string PaymentMethod { get; set; } = PaymentMethods.Cash;

        public string? TransactionReference { get; set; }

        public Guid? ApprovedByAdminID { get; set; }

        public DateTime ApprovedAt { get; set; }

        public DateTime PaidAt { get; set; }

        public DateTime FailedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AdminNotes { get; set; }

        public string? FailureReason { get; set; }


        public PickupRequest PickupRequest { get; set; }
        public ApplicationUser? RecipientUser { get; set; }
        //public ApplicationUser? ApprovedByAdmin { get; set; }
    }
}
