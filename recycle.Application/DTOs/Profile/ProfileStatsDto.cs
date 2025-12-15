using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Profile
{
    public class ProfileStatsDto
    {
        public int TotalRequests { get; set; }
        public int CompletedPickups { get; set; }
        public decimal TotalEarnings { get; set; }
        public int ImpactScore { get; set; }
    }
}
