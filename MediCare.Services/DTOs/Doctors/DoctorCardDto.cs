using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs.Doctors
{
    public sealed class DoctorCardDto
    {
        public Guid DoctorProfileId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string City { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;
        public decimal ConsultationFee { get; init; }
        public short SlotMinutes { get; init; }
        public DateTime? NextAvailableStartUtc { get; init; }
        public IReadOnlyList<string> Specializations { get; init; }
            = Array.Empty<string>();
    }
}
