using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.DriverAssignments
{
    public class DriverAssignmentResponseDto
    {
        public Guid AssignmentId { get; set; }
        public Guid RequestId { get; set; }
        public string PickupAddress { get; set; }//from another table PickupRequests
        public string DriverId { get; set; } 
        public string DriverName { get; set; }
        public string AdminName { get; set; }
        public AssignmentStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? DriverNotes { get; set; }
        public bool IsActive { get; set; }
    }
}
