using recycle.Application.DTOs.RequestMaterials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.PickupRequest
{
    public class WaitingRequestDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime PreferredPickupDate { get; set; }
        public DateTime PreferredPickupTime { get; set; }
        public AddressDto Address { get; set; } = new AddressDto();
        public string phoneNumber { get; set; } = string.Empty;
        public List<WaitingRequestMaterialDto> RequestMaterials { get; set; }
        public decimal TotalEstimatedWeight { get; set; }
        public string Status { get; set; }

    }
}
