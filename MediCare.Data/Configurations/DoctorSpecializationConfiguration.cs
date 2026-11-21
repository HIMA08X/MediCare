using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class DoctorSpecializationConfiguration
    : IEntityTypeConfiguration<DoctorSpecialization>
{
    public void Configure(EntityTypeBuilder<DoctorSpecialization> builder)
    {
        builder.HasKey(x => new
        {
            x.DoctorProfileId,
            x.SpecializationId
        });

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.Specializations)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Specialization)
            .WithMany(x => x.Doctors)
            .HasForeignKey(x => x.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}