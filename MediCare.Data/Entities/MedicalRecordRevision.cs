using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class MedicalRecordRevision
    {
        public Guid Id { get; set; }
        public Guid MedicalRecordId { get; set; }
        public Guid EditedByUserId { get; set; }
        public int RevisionNumber { get; set; }
        public string Symptoms { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentNotes { get; set; } = string.Empty;
        public DateTime EditedAtUtc { get; set; } = DateTime.UtcNow;
        public MedicalRecord MedicalRecord { get; set; } = null!;
        public ApplicationUser EditedByUser { get; set; } = null!;
    }
}
