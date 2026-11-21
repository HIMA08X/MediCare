using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs
{
    public sealed record BookAppointmentRequest(
    Guid DoctorProfileId,
    DateTime StartUtc,
    string? Reason);
}
