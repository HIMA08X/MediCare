using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace MediCare.Data.Persistence
{
    public sealed class MediCareDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public MediCareDbContext(DbContextOptions<MediCareDbContext> options) : base(options)
        {

        }
        public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
        public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<DoctorSpecialization> DoctorSpecializations
            => Set<DoctorSpecialization>();
        public DbSet<DoctorAvailability> DoctorAvailabilities
            => Set<DoctorAvailability>();
        public DbSet<DoctorTimeOff> DoctorTimeOffs => Set<DoctorTimeOff>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
        public DbSet<MedicalRecordRevision> MedicalRecordRevisions
            => Set<MedicalRecordRevision>();
        public DbSet<MedicalDocument> MedicalDocuments
            => Set<MedicalDocument>();
        public DbSet<Prescription> Prescriptions => Set<Prescription>();
        public DbSet<PrescriptionItem> PrescriptionItems
            => Set<PrescriptionItem>();
        public DbSet<PatientAllergy> PatientAllergies
            => Set<PatientAllergy>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationPreference> NotificationPreferences
            => Set<NotificationPreference>();
        public DbSet<ClinicSetting> ClinicSettings => Set<ClinicSetting>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityRole<Guid>>().HasData(
               new IdentityRole<Guid>
               {
                   Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                   Name = MediCareRoles.Patient,
                   NormalizedName = MediCareRoles.Patient.ToUpperInvariant()
               },
               new IdentityRole<Guid>
               {
                   Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                   Name = MediCareRoles.Doctor,
                   NormalizedName = MediCareRoles.Doctor.ToUpperInvariant()
               },
               new IdentityRole<Guid>
               {
                   Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                   Name = MediCareRoles.Admin,
                   NormalizedName = MediCareRoles.Admin.ToUpperInvariant()
               });

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.FullName)
                    .HasMaxLength(160)
                    .IsRequired();

                entity.Property(x => x.CreatedAtUtc)
                    .HasColumnType("datetime2");
            });

            modelBuilder.Entity<DoctorProfile>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasIndex(x => x.UserId).IsUnique();
                entity.HasIndex(x => x.LicenseNumber).IsUnique();
                entity.HasIndex(x => new { x.City, x.IsListed });

                entity.Property(x => x.LicenseNumber)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Bio).HasMaxLength(1000);
                entity.Property(x => x.City).HasMaxLength(80).IsRequired();

                entity.Property(x => x.ConsultationFee)
                    .HasPrecision(10, 2);

                entity.Property(x => x.ApprovalStatus)
                    .HasConversion<byte>();

                entity.HasOne(x => x.User)
                    .WithOne(x => x.DoctorProfile)
                    .HasForeignKey<DoctorProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PatientProfile>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.UserId).IsUnique();

                entity.Property(x => x.Gender).HasMaxLength(30);
                entity.Property(x => x.Address).HasMaxLength(200);
                entity.Property(x => x.AllergySummary).HasMaxLength(2000);
                entity.Property(x => x.MedicalHistorySummary).HasMaxLength(2000);
                entity.Property(x => x.EmergencyContact).HasMaxLength(120);

                entity.HasOne(x => x.User)
                    .WithOne(x => x.PatientProfile)
                    .HasForeignKey<PatientProfile>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Specialization>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();

                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<DoctorSpecialization>(entity =>
            {
                entity.HasKey(x => new { x.DoctorProfileId, x.SpecializationId });

                entity.HasOne(x => x.Doctor)
                    .WithMany(x => x.Specializations)
                    .HasForeignKey(x => x.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Specialization)
                    .WithMany(x => x.Doctors)
                    .HasForeignKey(x => x.SpecializationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DoctorAvailability>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.DoctorProfileId, x.DayOfWeek });

                entity.Property(x => x.DayOfWeek).HasConversion<byte>();

                entity.HasOne(x => x.Doctor)
                    .WithMany(x => x.Availabilities)
                    .HasForeignKey(x => x.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_DoctorAvailability_TimeRange",
                    "[EndTime] > [StartTime]"));
            });

            modelBuilder.Entity<DoctorTimeOff>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.DoctorProfileId, x.StartUtc, x.EndUtc });

                entity.Property(x => x.StartUtc).HasColumnType("datetime2");
                entity.Property(x => x.EndUtc).HasColumnType("datetime2");
                entity.Property(x => x.Reason).HasMaxLength(200);

                entity.HasOne(x => x.Doctor)
                    .WithMany(x => x.TimeOffs)
                    .HasForeignKey(x => x.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_DoctorTimeOff_TimeRange",
                    "[EndUtc] > [StartUtc]"));
            });

            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status).HasConversion<byte>();
                entity.Property(x => x.StartUtc).HasColumnType("datetime2");
                entity.Property(x => x.EndUtc).HasColumnType("datetime2");
                entity.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
                entity.Property(x => x.Reason).HasMaxLength(500);
                entity.Property(x => x.DecisionReason).HasMaxLength(300);
                entity.Property(x => x.FeeSnapshot).HasPrecision(10, 2);
                entity.Property(x => x.RowVersion).IsRowVersion();

                entity.HasIndex(x => new { x.DoctorProfileId, x.StartUtc })
                    .IsUnique()
                    .HasFilter("[Status] IN (0, 1)");

                entity.HasIndex(x => new { x.PatientProfileId, x.StartUtc });
                entity.HasIndex(x => new { x.DoctorProfileId, x.StartUtc, x.Status });

                entity.HasOne(x => x.Doctor)
                    .WithMany(x => x.Appointments)
                    .HasForeignKey(x => x.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Patient)
                    .WithMany(x => x.Appointments)
                    .HasForeignKey(x => x.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.PreviousAppointment)
                    .WithOne(x => x.RescheduledToAppointment)
                    .HasForeignKey<Appointment>(x => x.PreviousAppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.ToTable(table => table.HasCheckConstraint(
                    "CK_Appointments_TimeRange",
                    "[EndUtc] > [StartUtc]"));
            });

            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.AppointmentId).IsUnique();
                entity.HasIndex(x => new { x.PatientProfileId, x.CreatedAtUtc });

                entity.Property(x => x.Symptoms).HasMaxLength(4000);
                entity.Property(x => x.Diagnosis).HasMaxLength(4000);
                entity.Property(x => x.TreatmentNotes).HasMaxLength(8000);

                entity.HasOne(x => x.Appointment)
                    .WithOne(x => x.MedicalRecord)
                    .HasForeignKey<MedicalRecord>(x => x.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Patient)
                    .WithMany()
                    .HasForeignKey(x => x.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Doctor)
                    .WithMany()
                    .HasForeignKey(x => x.DoctorProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MedicalRecordRevision>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.MedicalRecordId, x.RevisionNumber })
                    .IsUnique();

                entity.Property(x => x.Symptoms).HasMaxLength(4000);
                entity.Property(x => x.Diagnosis).HasMaxLength(4000);
                entity.Property(x => x.TreatmentNotes).HasMaxLength(8000);

                entity.HasOne(x => x.MedicalRecord)
                    .WithMany(x => x.Revisions)
                    .HasForeignKey(x => x.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.EditedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.EditedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MedicalDocument>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.StorageKey).IsUnique();
                entity.HasIndex(x => x.PatientProfileId);

                entity.Property(x => x.StorageKey).HasMaxLength(200).IsRequired();
                entity.Property(x => x.OriginalFileName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Sha256).HasMaxLength(64).IsRequired();

                entity.HasOne(x => x.Patient)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.MedicalRecord)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.UploadedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.MedicalRecordId, x.Version }).IsUnique();

                entity.Property(x => x.Status).HasConversion<byte>();
                entity.Property(x => x.Instructions).HasMaxLength(2000);
                entity.Property(x => x.CancellationReason).HasMaxLength(500);

                entity.HasOne(x => x.MedicalRecord)
                    .WithMany(x => x.Prescriptions)
                    .HasForeignKey(x => x.MedicalRecordId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByDoctor)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByDoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.SupersedesPrescription)
                    .WithMany()
                    .HasForeignKey(x => x.SupersedesPrescriptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PrescriptionItem>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.PrescriptionId);

                entity.Property(x => x.MedicationName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Dosage).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Frequency).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Instructions).HasMaxLength(300);

                entity.HasOne(x => x.Prescription)
                    .WithMany(x => x.Items)
                    .HasForeignKey(x => x.PrescriptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PatientAllergy>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.PatientProfileId, x.Allergen });

                entity.Property(x => x.Allergen).HasMaxLength(150).IsRequired();
                entity.Property(x => x.Reaction).HasMaxLength(500);
                entity.Property(x => x.Notes).HasMaxLength(1000);

                entity.HasOne(x => x.Patient)
                    .WithMany(x => x.Allergies)
                    .HasForeignKey(x => x.PatientProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.DedupKey).IsUnique();
                entity.HasIndex(x => new { x.Status, x.ScheduledAtUtc });
                entity.HasIndex(x => new { x.UserId, x.CreatedAtUtc });

                entity.Property(x => x.Channel).HasConversion<byte>();
                entity.Property(x => x.Status).HasConversion<byte>();
                entity.Property(x => x.Type).HasMaxLength(80).IsRequired();
                entity.Property(x => x.Payload).HasMaxLength(2000).IsRequired();
                entity.Property(x => x.DedupKey).HasMaxLength(150).IsRequired();
                entity.Property(x => x.LastError).HasMaxLength(1000);

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Appointment)
                    .WithMany()
                    .HasForeignKey(x => x.AppointmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<NotificationPreference>(entity =>
            {
                entity.HasKey(x => x.UserId);

                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<NotificationPreference>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ClinicSetting>(entity =>
            {
                entity.HasKey(x => x.Key);

                entity.Property(x => x.Key).HasMaxLength(50);
                entity.Property(x => x.Value).HasMaxLength(500).IsRequired();
            });
            modelBuilder.Entity<ClinicSetting>().HasData(
                new ClinicSetting
                {
                    Key = "TimeZoneId",
                    Value = "Africa/Cairo",
                    UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClinicSetting
                {
                    Key = "DefaultSlotMinutes",
                    Value = "30",
                    UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClinicSetting
                {
                    Key = "CancelCutoffHours",
                    Value = "12",
                    UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClinicSetting
                {
                    Key = "ReminderLeadHours",
                    Value = "24",
                    UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new ClinicSetting
                {
                    Key = "MinimumBookingLeadHours",
                    Value = "2",
                    UpdatedAtUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.ActorUserId, x.OccurredAtUtc });
                entity.HasIndex(x => new { x.PatientProfileId, x.OccurredAtUtc });

                entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
                entity.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
                entity.Property(x => x.EntityId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.IpAddress).HasMaxLength(45);
                entity.Property(x => x.Details).HasMaxLength(500);

                entity.HasOne(x => x.ActorUser)
                    .WithMany()
                    .HasForeignKey(x => x.ActorUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}