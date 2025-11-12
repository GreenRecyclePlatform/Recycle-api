using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain
{
    public class ApplicationUser:IdentityUser
    {
        // Add to User.cs in recycle.Domain/Entities/User.cs
        public  ICollection<Review> ReviewsGiven { get; set; }
        public  ICollection<Review> ReviewsReceived { get; set; }
        public  ICollection<Notification> Notifications { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }


        public ICollection<PickupRequest>? pickupRequests { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Address>? Addresses { get; set; }
        public ICollection<Review>? Reviews { get; set; }

        public DriverProfile? DriverProfile { get; set; }

    }
}
