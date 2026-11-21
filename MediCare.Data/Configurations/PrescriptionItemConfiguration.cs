using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class PrescriptionItemConfiguration
    : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PrescriptionId);

        builder.Property(x => x.MedicationName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Dosage)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Frequency)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Instructions)
            .HasMaxLength(300);

        builder.HasOne(x => x.Prescription)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.PrescriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}