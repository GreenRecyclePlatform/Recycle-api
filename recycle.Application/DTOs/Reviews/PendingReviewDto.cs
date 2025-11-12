using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Reviews
{
    // Pending Review DTO
    public class PendingReviewDto
    {
        public int RequestId { get; set; }
        public string PickupAddress { get; set; }
        public DateTime CompletedAt { get; set; }
        public int DaysSinceCompletion { get; set; }
        public int DriverId { get; set; }
        public string DriverName { get; set; }
        public decimal DriverRating { get; set; }
    }
}
