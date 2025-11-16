using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Payment
{
    public class PaymentDto
    {
        public Guid PaymentId { get; set; } 

        public Guid RequestId { get; set; }
        public Guid RecipientUserId { get; set; }
        public string RecipientType { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? TransactionReference { get; set; }

        public string? PaymentStatus { get; set; }

        public string? AdminNotes { get; set; }
        public string? FailureReason { get; set; }
    }
}
