using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain;

namespace recycle.Infrastructure.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // Table Name
            builder.ToTable("Reviews");

            // Primary Key
            builder.HasKey(r => r.ReviewId);

            // Properties
            builder.Property(r => r.ReviewId)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Rating)
                .IsRequired()
                .HasAnnotation("CheckConstraint", "CHK_Reviews_Rating CHECK (Rating BETWEEN 1 AND 5)");

            builder.Property(r => r.Comment)
                .HasMaxLength(500)
                .IsUnicode(true);

            builder.Property(r => r.FlagReason)
                .HasMaxLength(200);

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships

            // Review -> PickupRequest (One-to-One)
            builder.HasOne(r => r.PickupRequest)
                .WithOne(pr => pr.Review)
                .HasForeignKey<Review>(r => r.RequestId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Reviewer/User (Many-to-One)
            builder.HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsGiven)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Reviewee/Driver (Many-to-One)
            builder.HasOne(r => r.Reviewee)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.RevieweeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(r => r.RequestId)
                .IsUnique()
                .HasDatabaseName("IX_Reviews_RequestId_Unique");

            builder.HasIndex(r => r.RevieweeId)
                .HasDatabaseName("IX_Reviews_RevieweeId");

            builder.HasIndex(r => r.ReviewerId)
                .HasDatabaseName("IX_Reviews_ReviewerId");

            builder.HasIndex(r => r.CreatedAt)
                .HasDatabaseName("IX_Reviews_CreatedAt");

            // Check Constraints
            builder.HasCheckConstraint("CHK_Reviews_NotSelfReview", "ReviewerId != RevieweeId");
        }
    }
}