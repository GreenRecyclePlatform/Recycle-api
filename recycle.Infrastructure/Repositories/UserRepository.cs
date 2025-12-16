using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public async Task SavePasswordResetTokenAsync(PasswordResetToken token)
        {
            var existingTokens = _context.PasswordResetTokens
                .Where(t => t.UserId == token.UserId && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
            _context.PasswordResetTokens.RemoveRange(existingTokens);

            _context.PasswordResetTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken> GetPasswordResetTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }
        public async Task<bool> UpdatePasswordAsync(Guid userId,string hashedPassword)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                user.PasswordHash = hashedPassword;
         
                await _userManager.UpdateAsync(user);

                return true;
            }
            return false;
        }

        public async Task<bool> MarkTokenAsUsedAsync(Guid userId)
        {
            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == userId && t.IsUsed == false);
            if (resetToken != null)
            {
                resetToken.IsUsed = true;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
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
        public async Task<ApplicationUser> GetByUserNameAsync(string userName)
        {
            var applicationUser = await _userManager.FindByNameAsync(userName);
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
