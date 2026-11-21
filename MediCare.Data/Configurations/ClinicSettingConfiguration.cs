using MediCare.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class ClinicSettingConfiguration
    : IEntityTypeConfiguration<ClinicSetting>
{
    public void Configure(EntityTypeBuilder<ClinicSetting> builder)
    {
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key)
            .HasMaxLength(50);

        builder.Property(x => x.Value)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasData(
            new ClinicSetting
            {
                Key = "TimeZoneId",
                Value = "Africa/Cairo",
                UpdatedAtUtc = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ClinicSetting
            {
                Key = "DefaultSlotMinutes",
                Value = "30",
                UpdatedAtUtc = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ClinicSetting
            {
                Key = "CancelCutoffHours",
                Value = "12",
                UpdatedAtUtc = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ClinicSetting
            {
                Key = "ReminderLeadHours",
                Value = "24",
                UpdatedAtUtc = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new ClinicSetting
            {
                Key = "MinimumBookingLeadHours",
                Value = "2",
                UpdatedAtUtc = new DateTime(
                    2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
    }
}