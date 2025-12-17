using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.supplier
{
    public class SupplierOrderResponseDto
    {
        public Guid OrderId { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierCompanyName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty; // Pending/Completed/Failed
        public string? StripePaymentIntentId { get; set; }
        public DateTime? PaidAt { get; set; } 
        public DateTime CreatedAt { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string MaterialIcon { get; set; } = "♻️";
        public decimal Quantity { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
