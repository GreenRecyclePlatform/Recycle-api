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
        IUserRepository Users { get; }
        IRepository<Address> Addresses { get; }
        IPaymentRepository Payments { get; }
        //IRepository<Payment> Payments { get; }        //the old

        Task SaveChangesAsync();
    }
}
