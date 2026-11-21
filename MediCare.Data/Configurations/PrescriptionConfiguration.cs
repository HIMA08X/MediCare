using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class PrescriptionConfiguration
    : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.MedicalRecordId,
            x.Version
        }).IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<byte>();

        builder.Property(x => x.Instructions)
            .HasMaxLength(2000);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.HasOne(x => x.MedicalRecord)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.MedicalRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreatedByDoctor)
            .WithMany()
            .HasForeignKey(x => x.CreatedByDoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SupersedesPrescription)
            .WithMany()
            .HasForeignKey(x => x.SupersedesPrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}