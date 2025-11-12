using recycle.Domain;
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
        IRepository<Review> Reviews { get; }
        IRepository<Notification> Notifications { get; }
        IRepository<ApplicationUser> Users { get; }
        IRepository<PickupRequest> PickupRequests { get; }
        IRepository<DriverAssignment> DriverAssignments { get; }
        
        Task SaveChangesAsync();
    }
}
