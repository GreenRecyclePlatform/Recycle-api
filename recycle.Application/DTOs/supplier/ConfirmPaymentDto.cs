using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.supplier
{
    public class ConfirmPaymentDto
    {
          [Required(ErrorMessage = "Order ID is required")]
            public Guid OrderId { get; set; }

            [Required(ErrorMessage = "Payment Intent ID is required")]
            public string PaymentIntentId { get; set; } = string.Empty;
        
    }
}
