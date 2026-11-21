using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class AppointmentConfiguration
    : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<byte>();

        builder.Property(x => x.StartUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.EndUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.CreatedAtUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.DecisionReason)
            .HasMaxLength(300);

        builder.Property(x => x.FeeSnapshot)
            .HasPrecision(10, 2);

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new
        {
            x.DoctorProfileId,
            x.StartUtc
        })
            .IsUnique()
            .HasFilter("[Status] IN (0, 1)");

        builder.HasIndex(x => new
        {
            x.PatientProfileId,
            x.StartUtc
        });

        builder.HasIndex(x => new
        {
            x.DoctorProfileId,
            x.StartUtc,
            x.Status
        });

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Appointments)
            .HasForeignKey(x => x.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PreviousAppointment)
            .WithOne(x => x.RescheduledToAppointment)
            .HasForeignKey<Appointment>(x => x.PreviousAppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_Appointments_TimeRange",
                "[EndUtc] > [StartUtc]"));
    }
}
