using System;

namespace recycle.Application.DTOs.Payment
{
    public class CreatePaymentDto
    {
        public Guid RecipientUserId { get; set; }
        public Guid RequestId { get; set; }
        public decimal Amount { get; set; }
        public string RecipientType { get; set; } = "User"; // ADDED: Track recipient type
        public string PaymentMethod { get; set; } = Domain.Enums.PaymentMethods.Wallet;
        public string? AdminNotes { get; set; }
    }
}