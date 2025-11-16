using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IReviewRepository Reviews { get; }
        INotificationRepository Notifications { get; }
        IUserRepository Users { get; }
        IRepository<Address> Addresses { get; }
        IRepository<PickupRequest> PickupRequests { get; }

        IDriverAssignmentRepository DriverAssignments { get; }
        IRepository<ApplicationUser> ApplicationUsers { get; }


        Task SaveChangesAsync();
    }
}