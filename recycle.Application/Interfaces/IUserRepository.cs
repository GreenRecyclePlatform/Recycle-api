using recycle.Application.Interfaces.IRepository;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Application.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<bool> IsUniqueAsync(string email, string userName);
        Task<ApplicationUser> GetByEmailAsync(string email);
        Task<ApplicationUser> GetByIdAsync(Guid id);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<ApplicationUser> AddUser(ApplicationUser user, string password);
    }
}
