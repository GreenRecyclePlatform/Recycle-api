using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using recycle.Application.DTOs;


namespace recycle.Application.DTOs.DriverAssignments
{
    public class DriverProfileResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string IdNumber { get; set; }
        public decimal Rating { get; set; }
        public int RatingCount { get; set; }
        public bool IsAvailable { get; set; }
        public int TotalTrips { get; set; }
        public DateTime CreatedAt { get; set; }
        public string phonenumber { get; set; }
         public string Email { get; set; }
        

        public AddressDto? Address { get; set; }
    }
}
