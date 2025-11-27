using System;
using System.ComponentModel.DataAnnotations;

namespace recycle.Application.DTOs.Materials
{
    public class MaterialDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Unit { get; set; }
        public string Icon { get; set; } = "♻️";
        public string? Image { get; set; }
        public decimal BuyingPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal PricePerKg { get; set; }
        public string Status { get; set; } = "active";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMaterialDto
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

        public string? Image { get; set; }

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

        public string? Image { get; set; }

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