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
    public class RequestMaterialConfiguration : IEntityTypeConfiguration<RequestMaterial>
    {
        public void Configure(EntityTypeBuilder<RequestMaterial> builder)
        {
            builder.ToTable("RequestMaterials");

            builder.Property(a => a.Id)
                .ValueGeneratedNever();

            builder.Property(rm => rm.EstimatedWeight)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("User's estimated weight in kg");

            builder.Property(rm => rm.ActualWeight)
                .HasPrecision(18, 2)
                .HasComment("Driver's actual measured weight in kg");

            builder.Property(rm => rm.PricePerKg)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Price snapshot at request creation time");

            builder.Property(rm => rm.TotalAmount)
                .HasPrecision(18, 2)
                .HasComment("Calculated as ActualWeight * PricePerKg");

            builder.Property(rm => rm.Notes)
                .HasMaxLength(500);

            builder.Property(rm => rm.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasCheckConstraint("CK_RequestMaterials_EstimatedWeight",
                "EstimatedWeight > 0");

            builder.HasCheckConstraint("CK_RequestMaterials_ActualWeight",
                "ActualWeight IS NULL OR ActualWeight >= 0");

            builder.HasCheckConstraint("CK_RequestMaterials_PricePerKg",
                "PricePerKg > 0");

            builder.HasIndex(rm => rm.RequestId);
            builder.HasIndex(rm => rm.MaterialId);
            builder.HasIndex(rm => new { rm.RequestId, rm.MaterialId }).IsUnique();

            builder.HasOne(rm => rm.PickupRequest)
                .WithMany(pr => pr.RequestMaterials)
                .HasForeignKey(rm => rm.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(rm => rm.Material)
                .WithMany(m => m.RequestMaterials)
                .HasForeignKey(rm => rm.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

}
