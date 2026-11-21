using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class MedicalRecordConfiguration
    : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.AppointmentId)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.PatientProfileId,
            x.CreatedAtUtc
        });

        builder.Property(x => x.Symptoms)
            .HasMaxLength(4000);

        builder.Property(x => x.Diagnosis)
            .HasMaxLength(4000);

        builder.Property(x => x.TreatmentNotes)
            .HasMaxLength(8000);

        builder.HasOne(x => x.Appointment)
            .WithOne(x => x.MedicalRecord)
            .HasForeignKey<MedicalRecord>(x => x.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Patient)
            .WithMany()
            .HasForeignKey(x => x.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}