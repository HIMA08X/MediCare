using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class MedicalDocumentConfiguration
    : IEntityTypeConfiguration<MedicalDocument>
{
    public void Configure(EntityTypeBuilder<MedicalDocument> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.StorageKey)
            .IsUnique();

        builder.HasIndex(x => x.PatientProfileId);

        builder.Property(x => x.StorageKey)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Sha256)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MedicalRecord)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.MedicalRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UploadedByUser)
            .WithMany()
            .HasForeignKey(x => x.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}