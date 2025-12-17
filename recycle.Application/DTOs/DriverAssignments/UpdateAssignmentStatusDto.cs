using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.DriverAssignments
{
    public class UpdateAssignmentStatusDto
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [Required]
        public AssignmentUpdateStatus Status { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
