using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        //add repositorys here
        public IUserRepository Users { get; private set; }
        public IRepository<Address> Addresses { get; private set; }
         public IRepository<Review> Reviews { get; }
        public IRepository<Notification> Notifications { get; }
         
        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IReviewRepository reviews,
            IRepository<Address> addresses, 
            INotificationRepository notifications,
            IRepository<ApplicationUser> users,
            IRepository<PickupRequest> pickupRequests,
            IRepository<DriverAssignment> driverAssignments
            IRepository<Address> addresses, IRepository<Review> reviews,
            IRepository<Notification> notifications, IUserRepository users
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;
            Reviews = (IReviewRepository?)reviews;
            Notifications = notifications;
            Users = users;
            PickupRequests = pickupRequests;
            DriverAssignments = driverAssignments;
            Users = users;

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
