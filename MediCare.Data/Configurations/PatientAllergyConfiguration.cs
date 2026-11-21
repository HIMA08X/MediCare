using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class PatientAllergyConfiguration
    : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.PatientProfileId,
            x.Allergen
        });

        builder.Property(x => x.Allergen)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Reaction)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Patient)
            .WithMany(x => x.Allergies)
            .HasForeignKey(x => x.PatientProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}