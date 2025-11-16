using Microsoft.AspNetCore.Identity;
using recycle.Application.Interfaces;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public UserRepository(
            AppDbContext context,
            UserManager<ApplicationUser> userManager) 
            : base(context)
        {
            _userManager = userManager;
             _context = context;
        }
        public async Task<ApplicationUser> AddUser(ApplicationUser user, string password)
        {
          
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var appUser = await _userManager.FindByIdAsync(user.Id.ToString());
            return appUser != null && await _userManager.CheckPasswordAsync(appUser, password);
        }

        public async Task<ApplicationUser> GetByEmailAsync(string email)
        {
            var applicationUser = await _userManager.FindByEmailAsync(email);
            if (applicationUser == null)
            {
                return null;
            }
            
            return applicationUser;
        }

        public async Task<ApplicationUser> GetByIdAsync(Guid id)
        {
            var applicationUser = await _userManager.FindByIdAsync(id.ToString());
            if (applicationUser == null)
            {
                return null;
            }
           return applicationUser;
        }

        public async Task<bool> IsUniqueAsync(string email, string userName)
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            if (userByEmail == null)
            {
                var userByUserName = await _userManager.FindByNameAsync(userName);
                if (userByUserName == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
