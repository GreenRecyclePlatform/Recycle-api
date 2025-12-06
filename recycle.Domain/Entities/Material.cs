using System;
using System.Collections.Generic;

namespace recycle.Domain.Entities
{
    public class Material
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Unit { get; set; } 
        public string Icon { get; set; } = "♻️";
        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }

        public decimal BuyingPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal PricePerKg { get; set; } 
        public string Status { get; set; } = "active";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<RequestMaterial> RequestMaterials { get; set; } = new List<RequestMaterial>();
    }
}