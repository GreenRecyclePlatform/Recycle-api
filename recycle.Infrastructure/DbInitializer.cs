using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure
{
    public class DbInitializer
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly AppDbContext _db;

        public DbInitializer(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, AppDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex) { }

            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole<Guid>("Admin")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole<Guid>("User")).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole<Guid>("Driver")).GetAwaiter().GetResult();

                var result = _userManager.CreateAsync(new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Admin",
                    Email = "admin@gmail.com",
                    UserName = "Admin77",
                }, "000000-Aa").GetAwaiter().GetResult();

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine(error.Description);
                    }
                }

                //ApplicationUser admin = _db.Users.FirstOrDefault(u => u.Email == "admin@gmail.com");
                ApplicationUser admin = await _userManager.FindByEmailAsync("admin@gmail.com");
                _userManager.AddToRoleAsync(admin, "Admin").GetAwaiter().GetResult();
            }

            return;
        }
    }
}
