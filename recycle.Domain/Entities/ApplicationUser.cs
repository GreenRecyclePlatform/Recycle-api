using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ✅ ADD THIS LINE

        public string? StripeAccountId { get; set; }
        public string? CompanyName { get; set; }


        public ICollection<PickupRequest>? pickupRequests { get; set; }
        public ICollection<Payment>? Payments { get; set; }
        public ICollection<Address>? Addresses { get; set; }
        public ICollection<Review> ReviewsGiven { get; set; }
        public ICollection<Review> ReviewsReceived { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<SupplierOrder>? SupplierOrders { get; set; }


        public DriverProfile? DriverProfile { get; set; }

        // Additional properties can be added here as needed
        public bool EmailNotifications { get; set; } = true;
        public bool SmsNotifications { get; set; } = false;
        public bool PickupReminders { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;

    }
}
