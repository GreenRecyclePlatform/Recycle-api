using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Reviews
{
    // Driver Rating Statistics DTO
    public class DriverRatingDto
    {
        public Guid DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStars { get; set; }
        public int FourStars { get; set; }
        public int ThreeStars { get; set; }
        public int TwoStars { get; set; }
        public int OneStar { get; set; }
        public int TotalPickups { get; set; }
        public double CompletionRate { get; set; }
    }

}
