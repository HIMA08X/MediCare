using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediCare.Data.Enums;

namespace MediCare.Data.Entities
{
    public sealed class Appointment
    {
        public Guid Id { get; set; }
        public Guid DoctorProfileId { get; set; }
        public Guid PatientProfileId { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public string? Reason { get; set; }
        public string? DecisionReason { get; set; }
        public decimal FeeSnapshot { get; set; }
        public Guid? PreviousAppointmentId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public byte[] RowVersion { get; set; } = [];

        public DoctorProfile Doctor { get; set; } = null!;
        public PatientProfile Patient { get; set; } = null!;
        public Appointment? PreviousAppointment { get; set; }
        public Appointment? RescheduledToAppointment { get; set; }

        public MedicalRecord? MedicalRecord { get; set; }
    }
}
