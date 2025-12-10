using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.supplier
{
    /// </summary>
    public class AvailableMaterialDto
    {
        public Guid MaterialId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Icon { get; set; } = "♻️";
        public string? ImageUrl { get; set; }
        public decimal SellingPrice { get; set; } // السعر للـ Supplier
        public string Unit { get; set; } = "kg";
        public bool IsActive { get; set; }
    }
}
