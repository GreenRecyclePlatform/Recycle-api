using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using recycle.Domain;
using recycle.Domain.Entities;
using recycle.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //add dbsets here
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<DriverAssignment> DriverAssignments { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<DriverProfile> DriverProfiles { get; set; }

        // DbSets for Materials and RequestMaterials
        public DbSet<Material> Materials { get; set; }
        public DbSet<RequestMaterial> RequestMaterials { get; set; }

        //add dbsets here
        public DbSet<PickupRequest> PickupRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(builder);

        }
    }
}