using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs
{
    public sealed record DoctorTimeOffConflictDto(
     Guid AppointmentId,
     string PatientFullName,
     DateTime StartUtc,
     DateTime EndUtc,
     string Status);

    public sealed record CreateDoctorTimeOffResultDto(
        Guid TimeOffId,
        IReadOnlyList<DoctorTimeOffConflictDto> AffectedAppointments);
}
