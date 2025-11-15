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
    internal class DriverProfileConfiguration : IEntityTypeConfiguration<DriverProfile>
    {
        public void Configure(EntityTypeBuilder<DriverProfile> builder)
        {
            builder.HasKey(dp => dp.Id);
            builder.Property(dp => dp.Id)
                .ValueGeneratedNever();
            builder.Property(dp => dp.UserId)
                .IsRequired();
            builder.Property(dp => dp.profileImageUrl)
                .IsRequired(true);
            builder.Property(dp => dp.idNumber)
                .IsRequired(true);
            builder.Property(dp => dp.CreatedAt).HasColumnType("datetime");
            builder.HasOne(dp=>dp.User)
                .WithOne(a=>a.DriverProfile)
                .HasForeignKey<DriverProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
