using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain;
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
            builder.ToTable("Materials");

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Description)
                .HasMaxLength(500);

            builder.Property(m => m.Unit)
                .HasMaxLength(20);

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

            builder.HasIndex(m => m.Name).IsUnique();
            builder.HasIndex(m => m.IsActive);

            builder.HasMany(m => m.RequestMaterials)
                .WithOne(rm => rm.Material)
                .HasForeignKey(rm => rm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
