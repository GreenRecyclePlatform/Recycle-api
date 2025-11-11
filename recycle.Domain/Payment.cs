namespace recycle.Domain
{
    public class Payment
    {
        public Guid ID { get; set; } = new Guid();

        public int RequestID { get; set; }

        public int RecipientUserID { get; set; }

        public string RecipientType { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string PaymentStatus { get; set; } = PaymentStatuses.Pending;

        public string PaymentMethod { get; set; } = PaymentMethods.Cash;

        public string? TransactionReference { get; set; }

        public int ApprovedByAdminID { get; set; }

        public DateTime ApprovedAt { get; set; }

        public DateTime PaidAt { get; set; }

        public DateTime FailedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? AdminNotes { get; set; }

        public string? FailureReason { get; set; }


        //public Request Request { get; set; } 
        //public User? RecipientUser { get; set; } 
        //public User? ApprovedByAdmin { get; set; } 
    }
}
