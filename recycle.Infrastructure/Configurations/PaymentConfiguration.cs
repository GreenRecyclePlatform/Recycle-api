using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using recycle.Domain.Entities;

namespace recycle.Infrastructure.Configurations
{
    internal class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.ID);

            builder.Property(p => p.ID).ValueGeneratedNever();

            builder.Property(p => p.Amount)
                   .HasColumnType("decimal(10,2)")
                   .IsRequired();

            builder.Property(p => p.RecipientType)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.PaymentStatus)
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(p => p.PaymentMethod)
                   .HasMaxLength(30)
                   .IsRequired();

            builder.Property(p => p.TransactionReference)
                   .HasMaxLength(100);

            builder.Property(p => p.AdminNotes)
                   .HasMaxLength(500);

            builder.Property(p => p.FailureReason)
                   .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            //builder.HasOne(p => p.Request)
            //       .WithMany()
            //       .HasForeignKey(p => p.RequestID)
            //       .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(p => p.RecipientUser)
            //       .WithMany()
            //       .HasForeignKey(p => p.RecipientUserID)
            //       .OnDelete(DeleteBehavior.Restrict);

            //builder.HasOne(p => p.ApprovedByAdmin)
            //       .WithMany()
            //       .HasForeignKey(p => p.ApprovedByAdminID)
            //       .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
