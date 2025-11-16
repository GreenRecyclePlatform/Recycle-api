using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IReviewRepository Reviews { get; }
        INotificationRepository Notifications { get; }
        IRepository<PickupRequest> PickupRequests { get; }
        IRepository<DriverAssignment> DriverAssignments { get; }
        IUserRepository Users { get; }
        IRepository<Address> Addresses { get; }
        IDriverAssignmentRepository DriverAssignments { get; }

        IRepository<ApplicationUser> Users { get; }
        IRepository<PickupRequest> PickupRequests { get; }

        Task SaveChangesAsync();
    }
}
