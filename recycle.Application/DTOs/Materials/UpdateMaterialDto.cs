using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace recycle.Application.DTOs.Materials
{
    public class UpdateMaterialDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Icon is required")]
        public string Icon { get; set; } = "♻️";

        public string? ImageUrl { get; set; }
        public string? ImageLocalPath { get; set; }
        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Buying price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Buying price must be greater than 0")]
        public decimal BuyingPrice { get; set; }

        [Required(ErrorMessage = "Selling price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Selling price must be greater than 0")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "Price per kg is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price per kg must be greater than 0")]
        public decimal PricePerKg { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(active|inactive)$", ErrorMessage = "Status must be either 'active' or 'inactive'")]
        public string Status { get; set; } = "active";
    }
}