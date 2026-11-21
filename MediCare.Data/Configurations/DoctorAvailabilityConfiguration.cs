using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class DoctorAvailabilityConfiguration
    : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.DoctorProfileId,
            x.DayOfWeek
        });

        builder.Property(x => x.DayOfWeek)
            .HasConversion<byte>();

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Availabilities)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_DoctorAvailability_TimeRange",
                "[EndTime] > [StartTime]"));
    }
}