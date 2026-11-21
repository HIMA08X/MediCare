using MediCare.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs
{
    public sealed record AppointmentSummaryDto(
     Guid Id,
     Guid DoctorProfileId,
     Guid PatientProfileId,
     string DoctorName,
     string PatientName,
     DateTime StartUtc,
     DateTime EndUtc,
     AppointmentStatus Status,
     decimal FeeSnapshot,
     string? Reason,
     string? DecisionReason);
}
