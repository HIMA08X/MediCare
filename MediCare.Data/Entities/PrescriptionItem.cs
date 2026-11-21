using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class PrescriptionItem
    {
        public Guid Id { get; set; }
        public Guid PrescriptionId { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public short DurationDays { get; set; }
        public string? Instructions { get; set; }
        public Prescription Prescription { get; set; } = null!;
    }
}
