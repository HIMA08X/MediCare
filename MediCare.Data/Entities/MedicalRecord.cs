using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class MedicalRecord
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid PatientProfileId { get; set; }
        public Guid DoctorProfileId { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentNotes { get; set; } = string.Empty;
        public int CurrentRevision { get; set; } = 1;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public Appointment Appointment { get; set; } = null!;
        public PatientProfile Patient { get; set; } = null!;
        public DoctorProfile Doctor { get; set; } = null!;

        public ICollection<MedicalRecordRevision> Revisions { get; set; }
            = new List<MedicalRecordRevision>();

        public ICollection<Prescription> Prescriptions { get; set; }
            = new List<Prescription>();

        public ICollection<MedicalDocument> Documents { get; set; }
            = new List<MedicalDocument>();
    }
}
