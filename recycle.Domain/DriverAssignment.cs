using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain
{
    public class DriverAssignment
    {
        [Required]

        [Key]
        public Guid AssignmentId { get; set; }
        [Required]
        public int RequestId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public int AssignedByAdminId { get; set; }

        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public AssignmentStatus Status { get; set; }
        // Assigned, Accepted, Rejected, InProgress, Completed

        [MaxLength(500)]
        public string? DriverNotes { get; set; }

        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        //public PickupRequest PickupRequest { get; set; }
        //public DriverProfile Driver { get; set; }
        //public ApplicationUser AssignedByAdmin { get; set; }
    }


}
