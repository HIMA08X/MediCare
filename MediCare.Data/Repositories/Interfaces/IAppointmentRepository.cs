using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<bool> HasOverlapAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default);

        Task<Appointment?> GetByIdAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default);

        Task<bool> HasOverlapExceptAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            Guid excludedAppointmentId,
            CancellationToken cancellationToken = default);
        Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetForPatientAsync(
            Guid patientProfileId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetForDoctorAsync(
            Guid doctorProfileId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Appointment>> GetOverlappingActiveAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default);
    }
}
