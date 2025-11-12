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

    }
}
