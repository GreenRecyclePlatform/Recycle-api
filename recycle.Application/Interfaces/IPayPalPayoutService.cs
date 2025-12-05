// File: recycle.Application/Interfaces/IPayPalPayoutService.cs
// FIXED: Return string instead of PayoutBatch to avoid ambiguity

using System;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IPayPalPayoutService
    {
        /// <summary>
        /// Send payout to recipient email via PayPal
        /// </summary>
        Task<PayoutResponse> SendPayoutAsync(
            string recipientEmail,
            decimal amount,
            string currency,
            string note);

        /// <summary>
        /// Get payout status by batch ID - returns status string
        /// </summary>
        Task<string> GetPayoutStatusAsync(string payoutBatchId);
    }

    /// <summary>
    /// Response model for PayPal payout operations
    /// </summary>
    public class PayoutResponse
    {
        public bool Success { get; set; }
        public string? PayoutBatchId { get; set; }
        public string? Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}