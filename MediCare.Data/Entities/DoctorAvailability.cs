using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class DoctorAvailability
    {
        public Guid Id { get; set; }
        public Guid DoctorProfileId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public TimeOnly? BreakStart { get; set; }
        public TimeOnly? BreakEnd { get; set; }
        public bool IsActive { get; set; } = true;

        public DoctorProfile Doctor { get; set; } = null!;
    }
}
