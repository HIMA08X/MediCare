using MediCare.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.Interfaces
{
    public interface IAppointmentService
    {
        Task<ServiceResult<Guid>> BookAsync(
            Guid patientUserId,
            BookAppointmentRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> ConfirmAsync(
            Guid doctorUserId,
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> RejectAsync(
            Guid doctorUserId,
            Guid appointmentId,
            string reason,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> CancelByPatientAsync(
            Guid patientUserId,
            Guid appointmentId,
            string reason,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> RescheduleByPatientAsync(
            Guid patientUserId,
            Guid appointmentId,
            RescheduleAppointmentRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> CompleteAsync(
            Guid doctorUserId,
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<Guid>> MarkNoShowAsync(
            Guid doctorUserId,
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PagedResult<AppointmentSummaryDto>>> GetPatientAppointmentsAsync(
            Guid patientUserId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<PagedResult<AppointmentSummaryDto>>> GetDoctorAppointmentsAsync(
            Guid doctorUserId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
