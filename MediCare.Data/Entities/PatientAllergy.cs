using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class PatientAllergy
    {
        public Guid Id { get; set; }
        public Guid PatientProfileId { get; set; }
        public string Allergen { get; set; } = string.Empty;
        public string? Reaction { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAtUtc { get; set; }
        public PatientProfile Patient { get; set; } = null!;
    }
}
