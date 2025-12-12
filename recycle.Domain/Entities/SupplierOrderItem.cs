using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    namespace recycle.Domain.Entities
    {
        public class SupplierOrderItem
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid OrderId { get; set; }
            public Guid MaterialId { get; set; }
            public decimal Quantity { get; set; } // بالكيلو
            public decimal PricePerKg { get; set; }
            public decimal TotalPrice { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            // Navigation Properties
            public SupplierOrder Order { get; set; } = null!;
            public Material Material { get; set; } = null!;
        }
    }
}
