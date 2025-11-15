using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.RequestMaterials
{
    public class RequestMaterialDto
    {
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public Guid MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public decimal EstimatedWeight { get; set; }
        public decimal? ActualWeight { get; set; }
        public decimal PricePerKg { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal EstimatedAmount => EstimatedWeight * PricePerKg;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateRequestMaterialDto
    {
        public Guid MaterialId { get; set; }
        public decimal EstimatedWeight { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateActualWeightDto
    {
        public decimal ActualWeight { get; set; }
        public string? Notes { get; set; }
    }

}
