using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Entities;
using MediCare.Data.Configurations;
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

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MediCareDbContext).Assembly);
        }
    }
}