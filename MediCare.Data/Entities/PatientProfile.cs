using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public class PatientProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? AllergySummary { get; set; }
        public string? MedicalHistorySummary { get; set; }
        public string? EmergencyContact { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();

        public ICollection<MedicalDocument> Documents { get; set; }
            = new List<MedicalDocument>();
        public ICollection<PatientAllergy> Allergies { get; set; }
            = new List<PatientAllergy>();
    }
}
