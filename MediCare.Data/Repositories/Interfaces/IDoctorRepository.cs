using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IDoctorRepository
    {
        Task<DoctorProfile?> GetByIdAsync(
            Guid doctorProfileId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DoctorAvailability>> GetAvailabilityForDayAsync(
            Guid doctorProfileId,
            DayOfWeek dayOfWeek,
            CancellationToken cancellationToken = default);

        Task<bool> HasTimeOffOverlapAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default);

        Task<DoctorProfile?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);

        Task ReplaceAvailabilityAsync(
            Guid doctorProfileId,
            IReadOnlyCollection<DoctorAvailability> availability,
            CancellationToken cancellationToken = default);

        Task AddTimeOffAsync(
            DoctorTimeOff timeOff,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteTimeOffAsync(
            Guid doctorProfileId,
            Guid timeOffId,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<DoctorProfile> Items, int TotalCount)> SearchPublicAsync(
            string? keyword,
            string? city,
            Guid? specializationId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<DoctorProfile?> GetPublicByIdAsync(
            Guid doctorProfileId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DoctorProfile>> SearchPublicCandidatesAsync(
            string? keyword,
            string? city,
            Guid? specializationId,
            CancellationToken cancellationToken = default);
    }
}
