using recycle.Application.Interfaces;
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
        public IRepository<Review> Reviews { get; }
        public IRepository<Notification> Notifications { get; }
        public IPaymentRepository Payments { get; }
        //public IRepository<Payment> Payments { get; }         //the old

        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IRepository<Address> addresses, IRepository<Review> reviews,
            IRepository<Notification> notifications, IUserRepository users, IRepository<Payment> payments
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;
            Reviews = reviews;
            Notifications = notifications;
            Users = users;
            Payments = payments;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
