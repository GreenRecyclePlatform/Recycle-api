using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain
{
    public class Material
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Unit { get; set; } // kg, ton, piece, etc.
        public decimal PricePerKg { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties 
        public ICollection<RequestMaterial> RequestMaterials { get; set; } = new List<RequestMaterial>();
    }
}
