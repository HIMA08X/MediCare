using MediCare.Data.Constants;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.Services
{
    public sealed class DoctorScheduleService : IDoctorScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DoctorScheduleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<bool>> SaveScheduleAsync(
            Guid doctorUserId,
            SaveDoctorScheduleRequest request,
            CancellationToken cancellationToken = default)
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
                doctorUserId,
                cancellationToken);

            if (doctor is null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "Doctor profile was not found.");
            }

            if (request.SlotMinutes is < 5 or > 120)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.ValidationFailed,
                    "Appointment duration must be between 5 and 120 minutes.");
            }

            var validationMessage = ValidateAvailability(request.Availability);

            if (validationMessage is not null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.ValidationFailed,
                    validationMessage);
            }

            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _unitOfWork.AcquireDoctorLockAsync(
                        doctor.Id,
                        token);

                    doctor.SlotMinutes = request.SlotMinutes;

                    var availability = request.Availability
                        .Select(item => new DoctorAvailability
                        {
                            DoctorProfileId = doctor.Id,
                            DayOfWeek = item.DayOfWeek,
                            StartTime = item.StartTime,
                            EndTime = item.EndTime,
                            BreakStart = item.BreakStart,
                            BreakEnd = item.BreakEnd,
                            IsActive = item.IsActive
                        })
                        .ToList();

                    await _unitOfWork.Doctors.ReplaceAvailabilityAsync(
                        doctor.Id,
                        availability,
                        token);

                    return ServiceResult<bool>.Success(true);
                },
                cancellationToken);
        }

        public async Task<ServiceResult<CreateDoctorTimeOffResultDto>> AddTimeOffAsync(
            Guid doctorUserId,
            CreateDoctorTimeOffRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.StartUtc.Kind != DateTimeKind.Utc ||
                request.EndUtc.Kind != DateTimeKind.Utc ||
                request.EndUtc <= request.StartUtc)
            {
                return ServiceResult<CreateDoctorTimeOffResultDto>.Failure(
                    ServiceError.ValidationFailed,
                    "Time-off dates must be valid UTC values and end after start.");
            }

            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
                doctorUserId,
                cancellationToken);

            if (doctor is null)
            {
                return ServiceResult<CreateDoctorTimeOffResultDto>.Failure(
                    ServiceError.NotFound,
                    "Doctor profile was not found.");
            }

            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _unitOfWork.AcquireDoctorLockAsync(
                        doctor.Id,
                        token);

                    var affectedAppointments =
                        await _unitOfWork.Appointments.GetOverlappingActiveAsync(
                            doctor.Id,
                            request.StartUtc,
                            request.EndUtc,
                            token);

                    var timeOff = new DoctorTimeOff
                    {
                        Id = Guid.NewGuid(),
                        DoctorProfileId = doctor.Id,
                        StartUtc = request.StartUtc,
                        EndUtc = request.EndUtc,
                        Reason = string.IsNullOrWhiteSpace(request.Reason)
                            ? null
                            : request.Reason.Trim()
                    };

                    await _unitOfWork.Doctors.AddTimeOffAsync(
                        timeOff,
                        token);

                    var affectedAppointmentDtos = affectedAppointments
                        .Select(appointment => new DoctorTimeOffConflictDto(
                            appointment.Id,
                            appointment.Patient.User.FullName,
                            appointment.StartUtc,
                            appointment.EndUtc,
                            appointment.Status.ToString()))
                        .ToArray();

                    return ServiceResult<CreateDoctorTimeOffResultDto>.Success(
                        new CreateDoctorTimeOffResultDto(
                            timeOff.Id,
                            affectedAppointmentDtos));
                },
                cancellationToken);
        }

        public async Task<ServiceResult<bool>> RemoveTimeOffAsync(
            Guid doctorUserId,
            Guid timeOffId,
            CancellationToken cancellationToken = default)
        {
            var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
                doctorUserId,
                cancellationToken);

            if (doctor is null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "Doctor profile was not found.");
            }

            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _unitOfWork.AcquireDoctorLockAsync(
                        doctor.Id,
                        token);

                    var removed = await _unitOfWork.Doctors.DeleteTimeOffAsync(
                        doctor.Id,
                        timeOffId,
                        token);

                    if (!removed)
                    {
                        return ServiceResult<bool>.Failure(
                            ServiceError.NotFound,
                            "Time-off entry was not found.");
                    }

                    return ServiceResult<bool>.Success(true);
                },
                cancellationToken);
        }

        public async Task<ServiceResult<IReadOnlyList<AvailableSlotDto>>>
            GetAvailableSlotsAsync(
                Guid doctorProfileId,
                DateOnly localDate,
                CancellationToken cancellationToken = default)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(
                doctorProfileId,
                cancellationToken);

            if (doctor is null ||
                doctor.ApprovalStatus != DoctorApprovalStatus.Approved ||
                !doctor.IsListed)
            {
                return ServiceResult<IReadOnlyList<AvailableSlotDto>>.Failure(
                    ServiceError.NotFound,
                    "Doctor was not found or is not available.");
            }

            var settings = await _unitOfWork.ClinicSettings.GetAllAsync(
                cancellationToken);

            if (!settings.TryGetValue(
                    ClinicSettingKeys.TimeZoneId,
                    out var timeZoneId) ||
                string.IsNullOrWhiteSpace(timeZoneId))
            {
                throw new InvalidOperationException(
                    "Clinic time zone setting is missing.");
            }

            var timeZone = FindTimeZone(timeZoneId);

            var minimumLeadHours = ReadSetting(
                settings,
                ClinicSettingKeys.MinimumBookingLeadHours,
                0,
                168);

            var availability =
                await _unitOfWork.Doctors.GetAvailabilityForDayAsync(
                    doctor.Id,
                    localDate.DayOfWeek,
                    cancellationToken);

            var localDateTime = localDate.ToDateTime(
                TimeOnly.MinValue,
                DateTimeKind.Unspecified);

            var slots = new List<AvailableSlotDto>();
            var nowUtc = DateTime.UtcNow.AddHours(minimumLeadHours);

            foreach (var workingPeriod in availability)
            {
                var current = workingPeriod.StartTime;

                while (current.AddMinutes(doctor.SlotMinutes) <=
                       workingPeriod.EndTime)
                {
                    var slotEnd = current.AddMinutes(doctor.SlotMinutes);

                    var overlapsBreak =
                        workingPeriod.BreakStart is not null &&
                        workingPeriod.BreakEnd is not null &&
                        current < workingPeriod.BreakEnd.Value &&
                        slotEnd > workingPeriod.BreakStart.Value;

                    if (!overlapsBreak)
                    {
                        var localStart = localDateTime.Add(current.ToTimeSpan());
                        var localEnd = localDateTime.Add(slotEnd.ToTimeSpan());

                        if (!timeZone.IsInvalidTime(localStart) &&
                            !timeZone.IsInvalidTime(localEnd))
                        {
                            var startUtc = TimeZoneInfo.ConvertTimeToUtc(
                                localStart,
                                timeZone);

                            var endUtc = TimeZoneInfo.ConvertTimeToUtc(
                                localEnd,
                                timeZone);

                            if (startUtc >= nowUtc)
                            {
                                var hasTimeOff =
                                    await _unitOfWork.Doctors.HasTimeOffOverlapAsync(
                                        doctor.Id,
                                        startUtc,
                                        endUtc,
                                        cancellationToken);

                                var hasAppointment =
                                    await _unitOfWork.Appointments.HasOverlapAsync(
                                        doctor.Id,
                                        startUtc,
                                        endUtc,
                                        cancellationToken);

                                if (!hasTimeOff && !hasAppointment)
                                {
                                    slots.Add(new AvailableSlotDto(
                                        startUtc,
                                        endUtc));
                                }
                            }
                        }
                    }

                    current = current.AddMinutes(doctor.SlotMinutes);
                }
            }

            return ServiceResult<IReadOnlyList<AvailableSlotDto>>.Success(
                slots.OrderBy(slot => slot.StartUtc).ToList());
        }

        private static string? ValidateAvailability(
            IReadOnlyList<DoctorAvailabilityRequest> availability)
        {
            foreach (var item in availability)
            {
                if (!item.IsActive)
                {
                    continue;
                }

                if (item.StartTime >= item.EndTime)
                {
                    return "Working period must end after it starts.";
                }

                if (item.BreakStart.HasValue != item.BreakEnd.HasValue)
                {
                    return "Break start and end must both be provided.";
                }

                if (item.BreakStart is not null &&
                    (item.BreakStart < item.StartTime ||
                     item.BreakEnd > item.EndTime ||
                     item.BreakStart >= item.BreakEnd))
                {
                    return "Break must be inside the working period.";
                }
            }

            var activePeriods = availability
                .Where(item => item.IsActive)
                .GroupBy(item => item.DayOfWeek);

            foreach (var day in activePeriods)
            {
                var ordered = day
                    .OrderBy(item => item.StartTime)
                    .ToList();

                for (var index = 1; index < ordered.Count; index++)
                {
                    if (ordered[index].StartTime < ordered[index - 1].EndTime)
                    {
                        return $"Working periods overlap on {day.Key}.";
                    }
                }
            }

            return null;
        }

        private static TimeZoneInfo FindTimeZone(string timeZoneId)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                var fallbackId = timeZoneId switch
                {
                    "Africa/Cairo" => "Egypt Standard Time",
                    "Egypt Standard Time" => "Africa/Cairo",
                    _ => null
                };

                if (fallbackId is null)
                {
                    throw new InvalidOperationException(
                        $"Time zone '{timeZoneId}' was not found.");
                }

                return TimeZoneInfo.FindSystemTimeZoneById(fallbackId);
            }
        }

        private static int ReadSetting(
            IReadOnlyDictionary<string, string> settings,
            string key,
            int minimum,
            int maximum)
        {
            if (!settings.TryGetValue(key, out var text) ||
                !int.TryParse(
                    text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out var value) ||
                value < minimum ||
                value > maximum)
            {
                throw new InvalidOperationException(
                    $"Clinic setting '{key}' is missing or invalid.");
            }

            return value;
        }
    }
}
