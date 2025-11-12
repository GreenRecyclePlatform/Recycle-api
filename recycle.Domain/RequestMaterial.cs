using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain
{
    public class RequestMaterial
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Guid MaterialId { get; set; }
        public decimal EstimatedWeight { get; set; }
        public decimal? ActualWeight { get; set; }
        public decimal PricePerKg { get; set; } // Snapshot of price at request time
        public decimal? TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (NO virtual keyword)
        public PickupRequest PickupRequest { get; set; } = null!;
        public Material Material { get; set; } = null!;
    }
}
