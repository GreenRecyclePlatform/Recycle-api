using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Payment
{
    public class CreatePaymentDto
    {
        public Guid RecipientUserId { get; set; }
        public Guid RequestId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = Domain.Enums.PaymentMethods.Wallet;
        public string? AdminNotes { get; set; }
    }
}
