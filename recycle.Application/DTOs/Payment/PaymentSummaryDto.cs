// recycle.Application/DTOs/Payment/PaymentSummaryDto.cs

namespace recycle.Application.DTOs.Payment
{
    public class PaymentSummaryDto
    {
        public decimal TotalEarnings { get; set; }
        public decimal TotalWithdrawals { get; set; }
        public decimal PendingAmount { get; set; }
        public decimal AvailableBalance { get; set; }
        public DateTime? LastPaymentDate { get; set; }
    }
}