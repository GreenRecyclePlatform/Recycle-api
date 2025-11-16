using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            // Table Name
            builder.ToTable("Notifications");

            // Primary Key
            builder.HasKey(n => n.NotificationId);

            // Properties Configuration
            builder.Property(n => n.NotificationId)
                .IsRequired()
                .HasDefaultValueSql("NEWID()"); // Auto-generate GUID in SQL Server

            builder.Property(n => n.UserId)
                .IsRequired();

            builder.Property(n => n.NotificationType)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(true);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500)
                .IsUnicode(true);

            builder.Property(n => n.RelatedEntityType)
                .HasMaxLength(50)
                .IsUnicode(true);

            builder.Property(n => n.RelatedEntityId)
                .IsRequired(false);

            builder.Property(n => n.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Normal")
                .IsUnicode(true);

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(n => n.ReadAt)
                .IsRequired(false);

            // Relationships

            // Notification -> ApplicationUser (Many-to-One)
            builder.HasOne(n => n.User)
                .WithMany() // Assuming ApplicationUser doesn't have a collection of Notifications
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Notifications_Users");

            // Indexes for Performance
            builder.HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notifications_UserId");

            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("IX_Notifications_UserId_IsRead");

            builder.HasIndex(n => n.CreatedAt)
                .HasDatabaseName("IX_Notifications_CreatedAt")
                .IsDescending();

            builder.HasIndex(n => new { n.RelatedEntityType, n.RelatedEntityId })
                .HasDatabaseName("IX_Notifications_RelatedEntity");

            builder.HasIndex(n => n.NotificationType)
                .HasDatabaseName("IX_Notifications_NotificationType");

            // Check Constraints
            builder.HasCheckConstraint(
                "CHK_Notifications_Priority",
                "[Priority] IN ('Low', 'Normal', 'High', 'Urgent')"
            );
        }
    }
}