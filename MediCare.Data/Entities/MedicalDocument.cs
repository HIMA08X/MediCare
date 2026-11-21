using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class MedicalDocument
    {
        public Guid Id { get; set; }
        public Guid PatientProfileId { get; set; }
        public Guid? MedicalRecordId { get; set; }
        public Guid UploadedByUserId { get; set; }
        public string StorageKey { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;
        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; }
        public PatientProfile Patient { get; set; } = null!;
        public MedicalRecord? MedicalRecord { get; set; }
        public ApplicationUser UploadedByUser { get; set; } = null!;
    }
}
