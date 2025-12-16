using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.supplier
{
    public class SupplierPaymentStatsDto
        {
            public decimal TotalRevenue { get; set; } 
            public int CompletedOrdersCount { get; set; } 
            public decimal PendingPayments { get; set; } 
            public List<SupplierPaymentDetailDto> RecentPayments { get; set; } 
        }

        public class SupplierPaymentDetailDto
        {
            public Guid OrderId { get; set; }
            public string SupplierName { get; set; }
            public decimal Amount { get; set; }
            public string PaymentStatus { get; set; }
            public DateTime? PaidAt { get; set; }
            public DateTime OrderDate { get; set; }
        
    }
}
