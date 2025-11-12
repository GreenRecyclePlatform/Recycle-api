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
        public Guid AssignmentId { get; set; } = Guid.NewGuid();
        public int RequestId { get; set; }
        public int DriverId { get; set; }
        public int AssignedByAdminId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public AssignmentStatus Status { get; set; }
        public string? DriverNotes { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        //public PickupRequest PickupRequest { get; set; }
        //public DriverProfile Driver { get; set; }
        //public ApplicationUser AssignedByAdmin { get; set; }
    }


}
