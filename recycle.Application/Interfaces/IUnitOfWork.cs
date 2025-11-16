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
        IPaymentRepository Payments { get; }
        //IRepository<Payment> Payments { get; }        //the old
        IRepository<PickupRequest> PickupRequests { get; }

        IDriverAssignmentRepository DriverAssignments { get; }


        Task SaveChangesAsync();
    }
}