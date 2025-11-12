using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Reviews
{
    // Review Response DTO
    public class ReviewDto
    {
        public Guid ReviewId { get; set; }
        public Guid RequestId { get; set; }
        public Guid ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public Guid RevieweeId { get; set; }
        public string RevieweeName { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsEdited { get; set; }
        public string PickupAddress { get; set; }
    }
}
