using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain;
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
            builder.HasKey(a => a.AssignmentId);
            builder.Property(a => a.AssignmentId)
                .ValueGeneratedNever();


            builder.HasIndex(da => new { da.RequestId, da.IsActive })
                   .IsUnique()
                   .HasFilter("[IsActive] = 1")
                   .HasDatabaseName("IX_DriverAssignments_RequestId_IsActive");

            // Convert enum to string in DB
            builder.Property(da => da.Status)
                   .IsRequired()
                   .HasMaxLength(50)
                   .HasConversion(
                       v => v.ToString(), // Enum -> string in DB
                       v => (AssignmentStatus)Enum.Parse(typeof(AssignmentStatus), v) // string -> Enum
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
            //builder.HasOne(da => da.PickupRequest)
            //       .WithMany(pr => pr.DriverAssignments)
            //       .HasForeignKey(da => da.RequestId)
            //       .OnDelete(DeleteBehavior.Restrict)
            //       .HasConstraintName("FK_DriverAssignments_PickupRequests");

            //builder.HasOne(da => da.Driver)
            //       .WithMany()
            //       .HasForeignKey(da => da.DriverId)
            //       .OnDelete(DeleteBehavior.Restrict)
            //       .HasConstraintName("FK_DriverAssignments_Driver");

            //builder.HasOne(da => da.AssignedByAdmin)
            //       .WithMany()
            //       .HasForeignKey(da => da.AssignedByAdminId)
            //       .OnDelete(DeleteBehavior.Restrict)
            //       .HasConstraintName("FK_DriverAssignments_Admin");

            builder.ToTable("DriverAssignments");
        }
    }
}




