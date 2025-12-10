using recycle.Domain.Entities.recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{


   
        public class SupplierOrder
        {
            public Guid OrderId { get; set; } = Guid.NewGuid();
            public Guid SupplierId { get; set; }
            public DateTime OrderDate { get; set; } = DateTime.UtcNow;
            public decimal TotalAmount { get; set; }
            public string PaymentStatus { get; set; } = "Pending"; // Pending/Completed/Failed
            public string? StripePaymentIntentId { get; set; }

            // ✅ الـ Columns الجديدة
            public DateTime? PaidAt { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }

            // ⚠️ اتركي دي موجودة لحد ما تتأكدي إن كل حاجة شغالة
            public string? OrderDetailsJson { get; set; }

            // ✅ Navigation Properties
            public ApplicationUser Supplier { get; set; }
            public ICollection<SupplierOrderItem> OrderItems { get; set; } = new List<SupplierOrderItem>();
        }
    }


