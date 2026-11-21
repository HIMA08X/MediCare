using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class DoctorTimeOff
    {
        public Guid Id { get; set; }
        public Guid DoctorProfileId { get; set; }

        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string? Reason { get; set; }

        public DoctorProfile Doctor { get; set; } = null!;
    }
}
