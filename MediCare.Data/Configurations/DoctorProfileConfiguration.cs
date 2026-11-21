using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class DoctorProfileConfiguration
    : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasIndex(x => x.LicenseNumber)
            .IsUnique();

        builder.HasIndex(x => new { x.City, x.IsListed });

        builder.Property(x => x.LicenseNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Bio)
            .HasMaxLength(1000);

        builder.Property(x => x.City)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.ConsultationFee)
            .HasPrecision(10, 2);

        builder.Property(x => x.ApprovalStatus)
            .HasConversion<byte>();

        builder.HasOne(x => x.User)
            .WithOne(x => x.DoctorProfile)
            .HasForeignKey<DoctorProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}