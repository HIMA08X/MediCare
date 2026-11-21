using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Enums;

namespace MediCare.Data.Entities
{
    public sealed class Prescription
    {
        public Guid Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public Guid CreatedByDoctorId { get; set; }
        public Guid? SupersedesPrescriptionId { get; set; }
        public int Version { get; set; } = 1;
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;
        public string? Instructions { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public MedicalRecord MedicalRecord { get; set; } = null!;
        public DoctorProfile CreatedByDoctor { get; set; } = null!;
        public Prescription? SupersedesPrescription { get; set; }

        public ICollection<PrescriptionItem> Items { get; set; }
            = new List<PrescriptionItem>();
    }
}
