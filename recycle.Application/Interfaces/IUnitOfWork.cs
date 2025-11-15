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
        IRepository<ApplicationUser> Users { get; }
        IRepository<PickupRequest> PickupRequests { get; }
        IRepository<DriverAssignment> DriverAssignments { get; }

        //add repository interfaces here
        IRepository<Address> Addresses { get; }

        Task SaveChangesAsync();
    }
}
