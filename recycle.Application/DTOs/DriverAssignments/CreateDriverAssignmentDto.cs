using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.DriverAssignments
{
    public class CreateDriverAssignmentDto
    {
        [Required(ErrorMessage = "Request ID is required")]
        public Guid RequestId { get; set; }

        [Required(ErrorMessage = "Driver ID is required")]
        public Guid DriverId { get; set; }
    }
}
