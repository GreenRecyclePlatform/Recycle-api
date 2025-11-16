using recycle.Application.Interfaces;
using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users { get; private set; }
        public IRepository<Address> Addresses { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public IRepository<PickupRequest> PickupRequests { get; private set; }

        public IDriverAssignmentRepository DriverAssignments { get; private set; }
        public IRepository<ApplicationUser> ApplicationUsers { get; private set; }


        public UnitOfWork(
            AppDbContext context,
            IReviewRepository reviews,
            IRepository<Address> addresses,
            INotificationRepository notifications,
            IRepository<PickupRequest> pickupRequests,
            IUserRepository users,
            IDriverAssignmentRepository driverAssignments,
                    IRepository<ApplicationUser> applicationUsers  

        )
        {
            _context = context;

            Addresses = addresses;
            Reviews = reviews;
            Notifications = notifications;
            Users = users;
            PickupRequests = pickupRequests;

            DriverAssignments = driverAssignments;
            ApplicationUsers = applicationUsers;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}