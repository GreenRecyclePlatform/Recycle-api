using recycle.Application.Interfaces;
using recycle.Domain;
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

        public UnitOfWork(AppDbContext context,
            //add repository parameters here
            IRepository<Address> addresses
            )
        {
            _context = context;
            //initialize repositories here
            Addresses = addresses;

        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
