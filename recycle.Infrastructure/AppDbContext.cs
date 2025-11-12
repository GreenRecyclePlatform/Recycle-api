using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using recycle.Domain;
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

        // DbSets for Materials and RequestMaterials
        public DbSet<Material> Materials { get; set; }
        public DbSet<RequestMaterial> RequestMaterials { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(builder);
            // matrials  configurations
            builder.ApplyConfiguration(new MaterialConfiguration());
            builder.ApplyConfiguration(new RequestMaterialConfiguration());


        }
    }
}
