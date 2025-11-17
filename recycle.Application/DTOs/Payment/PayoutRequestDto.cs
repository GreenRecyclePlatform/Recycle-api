using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Payment
{
    public class PayoutRequestDto
    {
        public Guid RecipientUserId { get; set; }
        public decimal Amount { get; set; }
        public Guid RequestId { get; set; }
        public string? Currency { get; set; }
    }
}
