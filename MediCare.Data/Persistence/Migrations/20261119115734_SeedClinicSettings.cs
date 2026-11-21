using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MediCare.Data.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedClinicSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ClinicSettings",
                columns: new[] { "Key", "UpdatedAtUtc", "Value" },
                values: new object[,]
                {
                    { "CancelCutoffHours", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "12" },
                    { "DefaultSlotMinutes", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30" },
                    { "MinimumBookingLeadHours", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2" },
                    { "ReminderLeadHours", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "24" },
                    { "TimeZoneId", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Africa/Cairo" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ClinicSettings",
                keyColumn: "Key",
                keyValue: "CancelCutoffHours");

            migrationBuilder.DeleteData(
                table: "ClinicSettings",
                keyColumn: "Key",
                keyValue: "DefaultSlotMinutes");

            migrationBuilder.DeleteData(
                table: "ClinicSettings",
                keyColumn: "Key",
                keyValue: "MinimumBookingLeadHours");

            migrationBuilder.DeleteData(
                table: "ClinicSettings",
                keyColumn: "Key",
                keyValue: "ReminderLeadHours");

            migrationBuilder.DeleteData(
                table: "ClinicSettings",
                keyColumn: "Key",
                keyValue: "TimeZoneId");
        }
    }
}
