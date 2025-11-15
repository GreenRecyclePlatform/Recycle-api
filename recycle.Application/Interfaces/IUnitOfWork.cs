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
        IRepository<Review> Reviews { get; }
        IRepository<Notification> Notifications { get; }        
        //add repository interfaces here
        IRepository<Address> Addresses { get; }

        Task SaveChangesAsync();
    }
}
