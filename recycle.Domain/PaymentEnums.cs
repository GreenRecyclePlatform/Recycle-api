namespace recycle.Domain
{
    public static class PaymentStatuses
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Paid = "Paid";
        public const string Failed = "Failed";
        public const string Rejected = "Rejected";
    }

    public static class PaymentMethods
    {
        public const string Cash = "Cash";
        public const string BankTransfer = "BankTransfer";
        public const string Wallet = "Wallet";
        public const string Check = "Check";
    }
}
