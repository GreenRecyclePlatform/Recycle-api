using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Configurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.Unit)
                .HasMaxLength(20);

            builder.Property(m => m.Icon)
                .HasMaxLength(50);

            builder.Property(m => m.Status)
                .HasMaxLength(20);

            // Price configurations
            builder.Property(m => m.BuyingPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Price paid when buying from users");

            builder.Property(m => m.SellingPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Price when selling to recyclers");

            builder.Property(m => m.PricePerKg)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Current price per kilogram");

            builder.Property(m => m.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(m => m.Name).IsUnique();
            builder.HasIndex(m => m.IsActive);

            // Relationships
            builder.HasMany(m => m.RequestMaterials)
                .WithOne(rm => rm.Material)
                .HasForeignKey(rm => rm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}