using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.RequestMaterials
{
    public class WaitingRequestMaterialDto
    {
        public string MaterialName { get; set; } = string.Empty;
        public decimal EstimatedWeight { get; set; }
    }
}
