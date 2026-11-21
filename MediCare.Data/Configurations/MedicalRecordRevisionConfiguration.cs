using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class MedicalRecordRevisionConfiguration
    : IEntityTypeConfiguration<MedicalRecordRevision>
{
    public void Configure(EntityTypeBuilder<MedicalRecordRevision> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.MedicalRecordId,
            x.RevisionNumber
        })
            .IsUnique();

        builder.Property(x => x.Symptoms)
            .HasMaxLength(4000);

        builder.Property(x => x.Diagnosis)
            .HasMaxLength(4000);

        builder.Property(x => x.TreatmentNotes)
            .HasMaxLength(8000);

        builder.HasOne(x => x.MedicalRecord)
            .WithMany(x => x.Revisions)
            .HasForeignKey(x => x.MedicalRecordId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.EditedByUser)
            .WithMany()
            .HasForeignKey(x => x.EditedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}