using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain;
namespace recycle.Infrastructure.Configurations;

public class PickupRequestConfiguration : IEntityTypeConfiguration<PickupRequest>
{
    public void Configure(EntityTypeBuilder<PickupRequest> builder)
    {
        builder.HasKey(pr => pr.RequestId);

        builder.Property(pr => pr.RequestId)
               .ValueGeneratedNever(); // Changed from ValueGeneratedOnAdd()

        builder.Property(pr => pr.UserId)
            .IsRequired();

        builder.Property(pr => pr.PickupAddress)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pr => pr.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pr => pr.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(pr => pr.PreferredPickupDate)
            .IsRequired();

        builder.Property(pr => pr.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Pending");

        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        builder.Property(pr => pr.TotalEstimatedWeight)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(pr => pr.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(pr => pr.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(pr => pr.CompletedAt)
            .IsRequired(false);

        // Relationships
        builder.HasOne(pr => pr.User)
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Important: Don't cascade delete requests when user is deleted

        builder.HasMany(pr => pr.RequestMaterials)
            .WithOne(rm => rm.PickupRequest)
            .HasForeignKey(rm => rm.RequestId)
            .OnDelete(DeleteBehavior.Cascade); // Delete materials when request is deleted

        builder.HasMany(pr => pr.DriverAssignments)
            .WithOne(da => da.PickupRequest)
            .HasForeignKey(da => da.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(pr => pr.Payments)
            .WithOne(p => p.PickupRequest)
            .HasForeignKey(p => p.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pr => pr.Review)
            .WithOne(r => r.PickupRequest)
            .HasForeignKey<Review>(r => r.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(pr => pr.UserId);
        builder.HasIndex(pr => pr.Status);
        builder.HasIndex(pr => pr.CreatedAt);
        builder.HasIndex(pr => pr.PreferredPickupDate);
    }
}