namespace recycle.Application.DTOs.Payment
{
    public class StripeTransferResult
    {
        public string TransferId { get; set; } = string.Empty;
        public string? PayoutId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
