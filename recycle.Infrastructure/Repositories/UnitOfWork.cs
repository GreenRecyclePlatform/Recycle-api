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
<<<<<<< HEAD
         public IRepository<Review> Reviews { get; }
        public IRepository<Notification> Notifications { get; }

        public IDriverAssignmentRepository DriverAssignments { get; private set; }

        public IRepository<ApplicationUser> Users { get; private set; }
        public IRepository<PickupRequest> PickupRequests { get; private set; }


        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IRepository<Address> addresses, IRepository<Review> reviews,
            IRepository<Notification> notifications,
            IDriverAssignmentRepository driverAssignments,
             IRepository<ApplicationUser> users,
            IRepository<PickupRequest> pickupRequests

=======
         public IReviewRepository Reviews { get; }
        public INotificationRepository Notifications { get; }
        public IRepository<PickupRequest> PickupRequests { get; }
        public IRepository<DriverAssignment> DriverAssignments { get; }

        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IReviewRepository reviews,
            IRepository<Address> addresses, 
            INotificationRepository notifications,
            IRepository<PickupRequest> pickupRequests,
            IRepository<DriverAssignment> driverAssignments,
            IUserRepository users
>>>>>>> origin/dev
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;
            Reviews = reviews;
            Notifications = notifications;
<<<<<<< HEAD
            DriverAssignments = driverAssignments; 
            Users = users;
            PickupRequests = pickupRequests;
=======
            Users = users;
            PickupRequests = pickupRequests;
            DriverAssignments = driverAssignments;
>>>>>>> origin/dev

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
