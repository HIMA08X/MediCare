using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Entities
{
    public sealed class DoctorSpecialization
    {
        public Guid DoctorProfileId { get; set; }
        public Guid SpecializationId { get; set; }

        public DoctorProfile Doctor { get; set; } = null!;
        public Specialization Specialization { get; set; } = null!;
    }
}
