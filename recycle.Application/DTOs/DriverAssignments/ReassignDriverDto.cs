using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.DriverAssignments
{
    public class ReassignDriverDto
    {
        [Required]  public Guid AssignmentId { get; set; }
        [Required(ErrorMessage = "New Driver ID is required")] public Guid NewDriverId { get; set; }
        [MaxLength(500)] public string? Reason { get; set; }
    }
}
