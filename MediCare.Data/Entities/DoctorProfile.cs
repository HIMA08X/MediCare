using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Enums;

namespace MediCare.Data.Entities
{
    public class DoctorProfile
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public short SlotMinutes { get; set; } = 30;
        public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.PendingApproval;
        public bool IsListed { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public ICollection<DoctorSpecialization> Specializations { get; set; }
            = new List<DoctorSpecialization>();

        public ICollection<DoctorAvailability> Availabilities { get; set; }
            = new List<DoctorAvailability>();

        public ICollection<DoctorTimeOff> TimeOffs { get; set; }
            = new List<DoctorTimeOff>();

        public ICollection<Appointment> Appointments { get; set; }
            = new List<Appointment>();
    }
}
