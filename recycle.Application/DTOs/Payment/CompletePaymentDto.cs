// recycle.Application/DTOs/Payment/CompletePaymentDto.cs

using System.ComponentModel.DataAnnotations;

namespace recycle.Application.DTOs.Payment
{
    public class CompletePaymentDto
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;
    }
}