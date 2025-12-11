using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using recycle.Domain.Entities;
using recycle.Domain.Entities.recycle.Domain.Entities;
using recycle.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
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
        public DbSet<Material> Materials { get; set; }
        public DbSet<RequestMaterial> RequestMaterials { get; set; }
        public DbSet<PickupRequest> PickupRequests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Setting> Settings { get; set; }


        public DbSet<SupplierOrder> SupplierOrders { get; set; }

        public DbSet<SupplierOrderItem> SupplierOrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<IdentityRole<Guid>>().HasData(
        new IdentityRole<Guid>
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
            Name = "Supplier",
            NormalizedName = "SUPPLIER"
        });

            builder.Entity<SupplierOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);

                entity.Property(e => e.PaymentStatus)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.TotalAmount)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Supplier)
                    .WithMany(u => u.SupplierOrders)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.SupplierId);
            });

            // ✅ Configuration للـ SupplierOrderItem
            builder.Entity<SupplierOrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Quantity)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.PricePerKg)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Material)
                    .WithMany(m => m.SupplierOrderItems)
                    .HasForeignKey(e => e.MaterialId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.MaterialId);
            });
        

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(builder);

            builder.Entity<DriverProfile>()
                .Property(d => d.Rating)
                .HasPrecision(18, 2);

        }
    }
}