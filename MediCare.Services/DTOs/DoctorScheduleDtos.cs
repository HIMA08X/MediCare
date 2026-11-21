using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs
{
    public sealed record DoctorAvailabilityRequest(
     DayOfWeek DayOfWeek,
     TimeOnly StartTime,
     TimeOnly EndTime,
     TimeOnly? BreakStart,
     TimeOnly? BreakEnd,
     bool IsActive);

    public sealed record SaveDoctorScheduleRequest(
        short SlotMinutes,
        IReadOnlyList<DoctorAvailabilityRequest> Availability);

    public sealed record CreateDoctorTimeOffRequest(
        DateTime StartUtc,
        DateTime EndUtc,
        string? Reason);

    public sealed record AvailableSlotDto(
        DateTime StartUtc,
        DateTime EndUtc);
}
