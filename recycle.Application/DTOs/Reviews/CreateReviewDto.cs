using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.Reviews
{
    public class CreateReviewDto
    {
        [Required(ErrorMessage = "RequestId is required")]
        public Guid RequestId { get; set; }
        [Required(ErrorMessage = "DriverId is required")]
        public Guid DriverId { get; set; }

        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        [MaxLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }
    }
}
