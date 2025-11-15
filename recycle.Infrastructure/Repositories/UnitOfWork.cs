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
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;
            Reviews = reviews;
            Notifications = notifications;
            Users = users;
            PickupRequests = pickupRequests;
            DriverAssignments = driverAssignments;

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
