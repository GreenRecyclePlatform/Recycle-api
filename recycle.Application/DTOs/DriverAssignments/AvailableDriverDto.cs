using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.DTOs.DriverAssignments
{
    public class AvailableDriverDto
    {
        public Guid DriverId { get; set; }
        public string DriverName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfileImageUrl { get; set; }
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsAvailable { get; set; }
        public int TotalTrips { get; set; }

        public AddressDto? Address { get; set; }
    }
}
