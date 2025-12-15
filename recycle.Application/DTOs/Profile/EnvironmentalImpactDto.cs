using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Profile
{
    public class EnvironmentalImpactDto
    {
        public decimal MaterialsRecycled { get; set; }
        public decimal Co2Saved { get; set; }
        public int TreesEquivalent { get; set; }
    }
}
