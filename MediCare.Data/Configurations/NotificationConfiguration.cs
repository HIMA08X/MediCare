using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DedupKey)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.Status,
            x.ScheduledAtUtc
        });

        builder.HasIndex(x => new
        {
            x.UserId,
            x.CreatedAtUtc
        });

        builder.Property(x => x.Channel)
            .HasConversion<byte>();

        builder.Property(x => x.Status)
            .HasConversion<byte>();

        builder.Property(x => x.Type)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.DedupKey)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(1000);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Appointment)
            .WithMany()
            .HasForeignKey(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}