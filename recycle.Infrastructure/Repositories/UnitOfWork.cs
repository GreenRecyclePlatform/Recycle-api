using recycle.Application.Interfaces;
using recycle.Domain;
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
        public IRepository<Review> Reviews { get; }
        public IRepository<Notification> Notifications { get; }
        public IRepository<ApplicationUser> Users { get; }
        public IRepository<PickupRequest> PickupRequests { get; }
        public IRepository<DriverAssignment> DriverAssignments { get; }

        public UnitOfWork(AppDbContext context,
            IRepository<Review> reviews,
            IRepository<Notification> notifications,
            IRepository<ApplicationUser> users,
            IRepository<PickupRequest> pickupRequests,
            IRepository<DriverAssignment> driverAssignments
            )
        {
            _context = context;
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
