using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class PatientProfileConfiguration
    : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.Property(x => x.Gender)
            .HasMaxLength(30);

        builder.Property(x => x.Address)
            .HasMaxLength(200);

        builder.Property(x => x.AllergySummary)
            .HasMaxLength(2000);

        builder.Property(x => x.MedicalHistorySummary)
            .HasMaxLength(2000);

        builder.Property(x => x.EmergencyContact)
            .HasMaxLength(120);

        builder.HasOne(x => x.User)
            .WithOne(x => x.PatientProfile)
            .HasForeignKey<PatientProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}