using MediCare.Data.Constants;
using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using MediCare.Services.DTOs;
using MediCare.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MediCare.Services.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult<Guid>> BookAsync(
        Guid patientUserId,
        BookAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DoctorProfileId == Guid.Empty)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "Doctor is required.");
        }

        if (request.StartUtc.Kind != DateTimeKind.Utc)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "Appointment time must be UTC.");
        }

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(
            request.DoctorProfileId,
            cancellationToken);

        if (doctor is null ||
            doctor.ApprovalStatus != DoctorApprovalStatus.Approved ||
            !doctor.IsListed)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.NotFound,
                "Doctor was not found or is not available.");
        }

        var patient = await _unitOfWork.Patients.GetByUserIdAsync(
            patientUserId,
            cancellationToken);

        if (patient is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.NotFound,
                "Patient profile was not found.");
        }

        var policy = await GetClinicBookingPolicyAsync(
            cancellationToken);

        var initialValidation = await ValidateBookingSlotAsync(
            doctor,
            request.StartUtc,
            policy,
            cancellationToken);

        if (!initialValidation.IsValid)
        {
            return ServiceResult<Guid>.Failure(
                initialValidation.Error,
                initialValidation.Message!);
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _unitOfWork.AcquireDoctorLockAsync(
                        doctor.Id,
                        token);

                    // نعيد التحقق بعد القفل حتى لا نعتمد على دوام
                    // أو إجازة تغيرت أثناء انتظار الطلب.
                    var lockedValidation = await ValidateBookingSlotAsync(
                        doctor,
                        request.StartUtc,
                        policy,
                        token);

                    if (!lockedValidation.IsValid)
                    {
                        return ServiceResult<Guid>.Failure(
                            lockedValidation.Error,
                            lockedValidation.Message!);
                    }

                    var endUtc = lockedValidation.EndUtc;

                    var hasOverlap =
                        await _unitOfWork.Appointments.HasOverlapAsync(
                            doctor.Id,
                            request.StartUtc,
                            endUtc,
                            token);

                    if (hasOverlap)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "The selected appointment time is no longer available.");
                    }

                    var appointment = new Appointment
                    {
                        Id = Guid.NewGuid(),
                        DoctorProfileId = doctor.Id,
                        PatientProfileId = patient.Id,
                        StartUtc = request.StartUtc,
                        EndUtc = endUtc,
                        Status = AppointmentStatus.Pending,
                        Reason = request.Reason,
                        FeeSnapshot = doctor.ConsultationFee,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    await _unitOfWork.Appointments.AddAsync(
                        appointment,
                        token);

                    await QueueInAppNotificationAsync(
                        doctor.UserId,
                        appointment.Id,
                        "NewAppointmentRequest",
                        $"appointment:{appointment.Id}:new-request:doctor",
                        token);

                    return ServiceResult<Guid>.Success(appointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            return CreateAppointmentConflict();
        }
    }

    public async Task<ServiceResult<Guid>> ConfirmAsync(
        Guid doctorUserId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
            doctorUserId,
            cancellationToken);

        if (doctor is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "Doctor profile was not found.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var appointment =
                        await _unitOfWork.Appointments.GetByIdAsync(
                            appointmentId,
                            token);

                    if (appointment is null)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.NotFound,
                            "Appointment was not found.");
                    }

                    if (appointment.DoctorProfileId != doctor.Id)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Forbidden,
                            "This appointment does not belong to this doctor.");
                    }

                    if (appointment.Status != AppointmentStatus.Pending)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "Only pending appointments can be confirmed.");
                    }

                    appointment.Status = AppointmentStatus.Confirmed;

                    await QueueInAppNotificationAsync(
                        appointment.Patient.UserId,
                        appointment.Id,
                        "AppointmentConfirmed",
                        $"appointment:{appointment.Id}:confirmed:patient",
                        token);

                    return ServiceResult<Guid>.Success(appointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
    }

    public async Task<ServiceResult<Guid>> RejectAsync(
        Guid doctorUserId,
        Guid appointmentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "A rejection reason is required.");
        }

        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
            doctorUserId,
            cancellationToken);

        if (doctor is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "Doctor profile was not found.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var appointment =
                        await _unitOfWork.Appointments.GetByIdAsync(
                            appointmentId,
                            token);

                    if (appointment is null)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.NotFound,
                            "Appointment was not found.");
                    }

                    if (appointment.DoctorProfileId != doctor.Id)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Forbidden,
                            "This appointment does not belong to this doctor.");
                    }

                    if (appointment.Status != AppointmentStatus.Pending)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "Only pending appointments can be rejected.");
                    }

                    appointment.Status = AppointmentStatus.Rejected;
                    appointment.DecisionReason = reason.Trim();

                    await QueueInAppNotificationAsync(
                        appointment.Patient.UserId,
                        appointment.Id,
                        "AppointmentRejected",
                        $"appointment:{appointment.Id}:rejected:patient",
                        token);

                    return ServiceResult<Guid>.Success(appointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
    }

    public async Task<ServiceResult<Guid>> CancelByPatientAsync(
        Guid patientUserId,
        Guid appointmentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var patient = await _unitOfWork.Patients.GetByUserIdAsync(
            patientUserId,
            cancellationToken);

        if (patient is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "Patient profile was not found.");
        }

        var policy = await GetClinicBookingPolicyAsync(
            cancellationToken);

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var appointment =
                        await _unitOfWork.Appointments.GetByIdAsync(
                            appointmentId,
                            token);

                    if (appointment is null)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.NotFound,
                            "Appointment was not found.");
                    }

                    if (appointment.PatientProfileId != patient.Id)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Forbidden,
                            "This appointment does not belong to this patient.");
                    }

                    if (appointment.Status != AppointmentStatus.Pending &&
                        appointment.Status != AppointmentStatus.Confirmed)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "This appointment can no longer be cancelled.");
                    }

                    var cancellationCutoffUtc = DateTime.UtcNow.AddHours(
                        policy.CancelCutoffHours);

                    if (appointment.StartUtc <= cancellationCutoffUtc)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            $"Cancellation must happen more than {policy.CancelCutoffHours} hours before the appointment.");
                    }

                    appointment.Status = AppointmentStatus.Cancelled;
                    appointment.DecisionReason =
                        string.IsNullOrWhiteSpace(reason)
                            ? null
                            : reason.Trim();

                    await QueueInAppNotificationAsync(
                        appointment.Doctor.UserId,
                        appointment.Id,
                        "AppointmentCancelled",
                        $"appointment:{appointment.Id}:cancelled:doctor",
                        token);

                    return ServiceResult<Guid>.Success(appointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
    }

    public async Task<ServiceResult<Guid>> RescheduleByPatientAsync(
        Guid patientUserId,
        Guid appointmentId,
        RescheduleAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (appointmentId == Guid.Empty)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "Appointment id is required.");
        }

        if (request.StartUtc.Kind != DateTimeKind.Utc)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "Appointment time must be UTC.");
        }

        var patient = await _unitOfWork.Patients.GetByUserIdAsync(
            patientUserId,
            cancellationToken);

        if (patient is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "Patient profile was not found.");
        }

        var policy = await GetClinicBookingPolicyAsync(
            cancellationToken);

        var oldAppointment = await _unitOfWork.Appointments.GetByIdAsync(
            appointmentId,
            cancellationToken);

        if (oldAppointment is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.NotFound,
                "Appointment was not found.");
        }

        if (oldAppointment.PatientProfileId != patient.Id)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "This appointment does not belong to this patient.");
        }

        if (!IsActiveAppointment(oldAppointment.Status))
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Conflict,
                "Only pending or confirmed appointments can be rescheduled.");
        }

        var doctor = await _unitOfWork.Doctors.GetByIdAsync(
            oldAppointment.DoctorProfileId,
            cancellationToken);

        if (doctor is null ||
            doctor.ApprovalStatus != DoctorApprovalStatus.Approved ||
            !doctor.IsListed)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Conflict,
                "The doctor is not currently available for booking.");
        }

        var initialValidation = await ValidateBookingSlotAsync(
            doctor,
            request.StartUtc,
            policy,
            cancellationToken);

        if (!initialValidation.IsValid)
        {
            return ServiceResult<Guid>.Failure(
                initialValidation.Error,
                initialValidation.Message!);
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    await _unitOfWork.AcquireDoctorLockAsync(
                        doctor.Id,
                        token);

                    if (!IsActiveAppointment(oldAppointment.Status))
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "The appointment changed and can no longer be rescheduled.");
                    }

                    var cancellationCutoffUtc = DateTime.UtcNow.AddHours(
                        policy.CancelCutoffHours);

                    if (oldAppointment.StartUtc <= cancellationCutoffUtc)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            $"Rescheduling must happen more than {policy.CancelCutoffHours} hours before the appointment.");
                    }

                    // نعيد التحقق بعد قفل الطبيب.
                    var lockedValidation = await ValidateBookingSlotAsync(
                        doctor,
                        request.StartUtc,
                        policy,
                        token);

                    if (!lockedValidation.IsValid)
                    {
                        return ServiceResult<Guid>.Failure(
                            lockedValidation.Error,
                            lockedValidation.Message!);
                    }

                    var newEndUtc = lockedValidation.EndUtc;

                    var hasOverlap =
                        await _unitOfWork.Appointments.HasOverlapExceptAsync(
                            doctor.Id,
                            request.StartUtc,
                            newEndUtc,
                            oldAppointment.Id,
                            token);

                    if (hasOverlap)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "The selected appointment time is no longer available.");
                    }

                    var newAppointment = new Appointment
                    {
                        Id = Guid.NewGuid(),
                        DoctorProfileId = oldAppointment.DoctorProfileId,
                        PatientProfileId = oldAppointment.PatientProfileId,
                        StartUtc = request.StartUtc,
                        EndUtc = newEndUtc,
                        Status = AppointmentStatus.Pending,
                        Reason = oldAppointment.Reason,
                        FeeSnapshot = doctor.ConsultationFee,
                        PreviousAppointmentId = oldAppointment.Id,
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    oldAppointment.Status = AppointmentStatus.Rescheduled;

                    await _unitOfWork.Appointments.AddAsync(
                        newAppointment,
                        token);

                    await QueueInAppNotificationAsync(
                        doctor.UserId,
                        newAppointment.Id,
                        "AppointmentRescheduled",
                        $"appointment:{newAppointment.Id}:rescheduled:doctor",
                        token);

                    return ServiceResult<Guid>.Success(newAppointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            return CreateAppointmentConflict();
        }
    }

    public Task<ServiceResult<Guid>> CompleteAsync(
        Guid doctorUserId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return UpdateDoctorAppointmentStatusAsync(
            doctorUserId,
            appointmentId,
            AppointmentStatus.Completed,
            "AppointmentCompleted",
            cancellationToken);
    }

    public Task<ServiceResult<Guid>> MarkNoShowAsync(
        Guid doctorUserId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        return UpdateDoctorAppointmentStatusAsync(
            doctorUserId,
            appointmentId,
            AppointmentStatus.NoShow,
            "AppointmentNoShow",
            cancellationToken);
    }

    private async Task<ServiceResult<Guid>> UpdateDoctorAppointmentStatusAsync(
        Guid doctorUserId,
        Guid appointmentId,
        AppointmentStatus targetStatus,
        string notificationType,
        CancellationToken cancellationToken)
    {
        if (appointmentId == Guid.Empty)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.ValidationFailed,
                "Appointment id is required.");
        }

        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
            doctorUserId,
            cancellationToken);

        if (doctor is null)
        {
            return ServiceResult<Guid>.Failure(
                ServiceError.Forbidden,
                "Doctor profile was not found.");
        }

        try
        {
            return await _unitOfWork.ExecuteInTransactionAsync(
                async token =>
                {
                    var appointment =
                        await _unitOfWork.Appointments.GetByIdAsync(
                            appointmentId,
                            token);

                    if (appointment is null)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.NotFound,
                            "Appointment was not found.");
                    }

                    if (appointment.DoctorProfileId != doctor.Id)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Forbidden,
                            "This appointment does not belong to this doctor.");
                    }

                    if (appointment.Status != AppointmentStatus.Confirmed)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "Only confirmed appointments can be completed or marked as no-show.");
                    }

                    if (appointment.StartUtc > DateTime.UtcNow)
                    {
                        return ServiceResult<Guid>.Failure(
                            ServiceError.Conflict,
                            "The appointment cannot be updated before its start time.");
                    }

                    appointment.Status = targetStatus;

                    await QueueInAppNotificationAsync(
                        appointment.Patient.UserId,
                        appointment.Id,
                        notificationType,
                        $"appointment:{appointment.Id}:{targetStatus}:patient",
                        token);

                    return ServiceResult<Guid>.Success(appointment.Id);
                },
                cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return CreateAppointmentConflict();
        }
    }

    public async Task<ServiceResult<PagedResult<AppointmentSummaryDto>>>
        GetPatientAppointmentsAsync(
            Guid patientUserId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var patient = await _unitOfWork.Patients.GetByUserIdAsync(
            patientUserId,
            cancellationToken);

        if (patient is null)
        {
            return ServiceResult<PagedResult<AppointmentSummaryDto>>.Failure(
                ServiceError.NotFound,
                "Patient profile was not found.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _unitOfWork.Appointments.GetForPatientAsync(
            patient.Id,
            page,
            pageSize,
            cancellationToken);

        var items = result.Items
            .Select(MapAppointmentSummary)
            .ToList();

        var pagedResult = new PagedResult<AppointmentSummaryDto>(
            items,
            page,
            pageSize,
            result.TotalCount);

        return ServiceResult<PagedResult<AppointmentSummaryDto>>.Success(
            pagedResult);
    }

    public async Task<ServiceResult<PagedResult<AppointmentSummaryDto>>>
        GetDoctorAppointmentsAsync(
            Guid doctorUserId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var doctor = await _unitOfWork.Doctors.GetByUserIdAsync(
            doctorUserId,
            cancellationToken);

        if (doctor is null)
        {
            return ServiceResult<PagedResult<AppointmentSummaryDto>>.Failure(
                ServiceError.NotFound,
                "Doctor profile was not found.");
        }

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _unitOfWork.Appointments.GetForDoctorAsync(
            doctor.Id,
            page,
            pageSize,
            cancellationToken);

        var items = result.Items
            .Select(MapAppointmentSummary)
            .ToList();

        var pagedResult = new PagedResult<AppointmentSummaryDto>(
            items,
            page,
            pageSize,
            result.TotalCount);

        return ServiceResult<PagedResult<AppointmentSummaryDto>>.Success(
            pagedResult);
    }

    private async Task<SlotValidation> ValidateBookingSlotAsync(
        DoctorProfile doctor,
        DateTime startUtc,
        ClinicBookingPolicy policy,
        CancellationToken cancellationToken)
    {
        if (startUtc.Kind != DateTimeKind.Utc)
        {
            return InvalidSlot(
                ServiceError.ValidationFailed,
                "Appointment time must be UTC.");
        }

        var earliestAllowedStartUtc = DateTime.UtcNow.AddHours(
            policy.MinimumBookingLeadHours);

        if (startUtc < earliestAllowedStartUtc)
        {
            return InvalidSlot(
                ServiceError.ValidationFailed,
                $"Appointment must be booked at least {policy.MinimumBookingLeadHours} hours in advance.");
        }

        if (doctor.SlotMinutes <= 0)
        {
            return InvalidSlot(
                ServiceError.ValidationFailed,
                "Doctor appointment duration is invalid.");
        }

        var endUtc = startUtc.AddMinutes(doctor.SlotMinutes);

        var localStart = TimeZoneInfo.ConvertTimeFromUtc(
            startUtc,
            policy.TimeZone);

        var localEnd = TimeZoneInfo.ConvertTimeFromUtc(
            endUtc,
            policy.TimeZone);

        if (localStart.Date != localEnd.Date)
        {
            return InvalidSlot(
                ServiceError.ValidationFailed,
                "Appointment cannot cross the clinic's local day boundary.");
        }

        var availability =
            await _unitOfWork.Doctors.GetAvailabilityForDayAsync(
                doctor.Id,
                localStart.DayOfWeek,
                cancellationToken);

        var localStartTime = TimeOnly.FromDateTime(localStart);
        var localEndTime = TimeOnly.FromDateTime(localEnd);

        var fitsSchedule = availability.Any(item =>
            FitsAvailability(
                item,
                localStartTime,
                localEndTime,
                doctor.SlotMinutes));

        if (!fitsSchedule)
        {
            return InvalidSlot(
                ServiceError.ValidationFailed,
                "The selected time is outside the doctor's working hours.");
        }

        var overlapsTimeOff =
            await _unitOfWork.Doctors.HasTimeOffOverlapAsync(
                doctor.Id,
                startUtc,
                endUtc,
                cancellationToken);

        if (overlapsTimeOff)
        {
            return InvalidSlot(
                ServiceError.Conflict,
                "The doctor is unavailable during the selected time.");
        }

        return new SlotValidation(
            true,
            endUtc,
            default,
            null);
    }

    private async Task<ClinicBookingPolicy> GetClinicBookingPolicyAsync(
        CancellationToken cancellationToken)
    {
        var settings = await _unitOfWork.ClinicSettings.GetAllAsync(
            cancellationToken);

        var timeZoneId = GetRequiredSetting(
            settings,
            ClinicSettingKeys.TimeZoneId);

        var timeZone = FindClinicTimeZone(timeZoneId);

        var cancelCutoffHours = ReadIntSetting(
            settings,
            ClinicSettingKeys.CancelCutoffHours,
            minimum: 0,
            maximum: 72);

        var minimumBookingLeadHours = ReadIntSetting(
            settings,
            ClinicSettingKeys.MinimumBookingLeadHours,
            minimum: 0,
            maximum: 168);

        return new ClinicBookingPolicy(
            timeZone,
            cancelCutoffHours,
            minimumBookingLeadHours);
    }

    private async Task QueueInAppNotificationAsync(
        Guid userId,
        Guid appointmentId,
        string type,
        string dedupKey,
        CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AppointmentId = appointmentId,
            Channel = NotificationChannel.InApp,
            Status = NotificationStatus.Pending,
            Type = type,
            Payload = System.Text.Json.JsonSerializer.Serialize(
                new { appointmentId }),
            DedupKey = dedupKey,
            ScheduledAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.AddNotificationAsync(
            notification,
            cancellationToken);
    }

    private static AppointmentSummaryDto MapAppointmentSummary(
        Appointment appointment)
    {
        return new AppointmentSummaryDto(
            appointment.Id,
            appointment.DoctorProfileId,
            appointment.PatientProfileId,
            appointment.Doctor.User.FullName,
            appointment.Patient.User.FullName,
            appointment.StartUtc,
            appointment.EndUtc,
            appointment.Status,
            appointment.FeeSnapshot,
            appointment.Reason,
            appointment.DecisionReason);
    }

    private static bool FitsAvailability(
        DoctorAvailability availability,
        TimeOnly startTime,
        TimeOnly endTime,
        short slotMinutes)
    {
        if (startTime < availability.StartTime ||
            endTime > availability.EndTime)
        {
            return false;
        }

        if (availability.BreakStart is not null &&
            availability.BreakEnd is not null &&
            startTime < availability.BreakEnd.Value &&
            endTime > availability.BreakStart.Value)
        {
            return false;
        }

        var offsetFromScheduleStart =
            startTime.ToTimeSpan() -
            availability.StartTime.ToTimeSpan();

        var slotDuration = TimeSpan.FromMinutes(slotMinutes);

        return offsetFromScheduleStart >= TimeSpan.Zero &&
               offsetFromScheduleStart.Ticks % slotDuration.Ticks == 0;
    }

    private static TimeZoneInfo FindClinicTimeZone(string timeZoneId)
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
                    $"Clinic time zone '{timeZoneId}' is not supported on this server.");
            }

            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(fallbackId);
            }
            catch (TimeZoneNotFoundException exception)
            {
                throw new InvalidOperationException(
                    $"Clinic time zone '{timeZoneId}' could not be found.",
                    exception);
            }
        }
    }

    private static string GetRequiredSetting(
        IReadOnlyDictionary<string, string> settings,
        string key)
    {
        if (!settings.TryGetValue(key, out var value) ||
            string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Required clinic setting '{key}' is missing.");
        }

        return value.Trim();
    }

    private static int ReadIntSetting(
        IReadOnlyDictionary<string, string> settings,
        string key,
        int minimum,
        int maximum)
    {
        var value = GetRequiredSetting(settings, key);

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result) ||
            result < minimum ||
            result > maximum)
        {
            throw new InvalidOperationException(
                $"Clinic setting '{key}' must be a number between {minimum} and {maximum}.");
        }

        return result;
    }

    private static bool IsActiveAppointment(AppointmentStatus status)
    {
        return status is AppointmentStatus.Pending or
            AppointmentStatus.Confirmed;
    }

    private static SlotValidation InvalidSlot(
        ServiceError error,
        string message)
    {
        return new SlotValidation(
            false,
            default,
            error,
            message);
    }

    private static ServiceResult<Guid> CreateAppointmentConflict()
    {
        return ServiceResult<Guid>.Failure(
            ServiceError.Conflict,
            "The appointment changed or the selected slot is no longer available. Refresh and try again.");
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.GetBaseException() is SqlException sqlException &&
               sqlException.Number is 2601 or 2627;
    }

    private sealed record ClinicBookingPolicy(
        TimeZoneInfo TimeZone,
        int CancelCutoffHours,
        int MinimumBookingLeadHours);

    private readonly record struct SlotValidation(
        bool IsValid,
        DateTime EndUtc,
        ServiceError Error,
        string? Message);
}