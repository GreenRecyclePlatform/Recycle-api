// recycle.Application/DTOs/Payment/PaymentFilterDto.cs

namespace recycle.Application.DTOs.Payment
{
    public class PaymentFilterDto
    {
        public Guid? UserId { get; set; }
        public string? Type { get; set; } // Earning, Withdrawal, Refund
        public string? Status { get; set; } // Pending, Processing, Completed, Failed, Cancelled
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}