using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class DoctorTimeOffConfiguration
    : IEntityTypeConfiguration<DoctorTimeOff>
{
    public void Configure(EntityTypeBuilder<DoctorTimeOff> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.DoctorProfileId,
            x.StartUtc,
            x.EndUtc
        });

        builder.Property(x => x.StartUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.EndUtc)
            .HasColumnType("datetime2");

        builder.Property(x => x.Reason)
            .HasMaxLength(200);

        builder.HasOne(x => x.Doctor)
            .WithMany(x => x.TimeOffs)
            .HasForeignKey(x => x.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
            table.HasCheckConstraint(
                "CK_DoctorTimeOff_TimeRange",
                "[EndUtc] > [StartUtc]"));
    }
}