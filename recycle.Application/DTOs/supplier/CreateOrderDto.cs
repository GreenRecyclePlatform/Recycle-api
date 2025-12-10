using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.supplier
{
    public class CreateSupplierOrderDto
    {
        [Required(ErrorMessage = "At least one item is required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemInputDto> Items { get; set; } = new();
    }

    public class OrderItemInputDto
    {
        [Required(ErrorMessage = "Material ID is required")]
        public Guid MaterialId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
       // [Range(0.1, 10000, ErrorMessage = "Quantity must be between 0.1 and 10000 kg")]
        public decimal Quantity { get; set; }
    }
}
