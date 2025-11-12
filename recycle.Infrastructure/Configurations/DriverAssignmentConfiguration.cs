using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain.Entities;
using recycle.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace recycle.Infrastructure.Configurations
{
    public class DriverAssignmentConfiguration : IEntityTypeConfiguration<DriverAssignment>
    {
        public void Configure(EntityTypeBuilder<DriverAssignment> builder)
        {
            // Primary Key
            builder.HasKey(a => a.AssignmentId);
            builder.Property(a => a.AssignmentId)
                   .ValueGeneratedNever(); 

            // Index
            builder.HasIndex(da => new { da.RequestId, da.IsActive })
                   .IsUnique()           


                   .HasFilter("[IsActive] = 1")
                   .HasDatabaseName("IX_DriverAssignments_RequestId_IsActive");

            // Fields


            builder.Property(da => da.RequestId).IsRequired();
            builder.Property(da => da.DriverId).IsRequired();
            builder.Property(da => da.AssignedByAdminId).IsRequired();

            builder.Property(da => da.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasConversion(
                       v => v.ToString(),
                       v => (AssignmentStatus)Enum.Parse(typeof(AssignmentStatus), v)
                   );

            builder.Property(da => da.DriverNotes)
                   .HasMaxLength(500);

            builder.Property(da => da.AssignedAt)
                   .IsRequired()
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(da => da.IsActive)
                   .IsRequired()
                   .HasDefaultValue(true);

            // Relationships 
            builder.HasOne(da => da.PickupRequest)
                   .WithMany(pr => pr.DriverAssignments)
                   .HasForeignKey(da => da.RequestId)
                  .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(da => da.Driver)
                   .WithMany().IsRequired()
                   .HasForeignKey(da => da.DriverId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(da => da.AssignedByAdmin)
                  .WithMany().IsRequired()
                  .HasForeignKey(da => da.AssignedByAdminId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}




