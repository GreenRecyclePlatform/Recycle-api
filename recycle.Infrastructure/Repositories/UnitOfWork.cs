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
        public IRepository<Address> Addresses { get; private set; }
         public IRepository<Review> Reviews { get; }
        public IRepository<Notification> Notifications { get; }
         
        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IRepository<Address> addresses, IRepository<Review> reviews,
            IRepository<Notification> notifications
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;
            Reviews = reviews;
            Notifications = notifications;

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
