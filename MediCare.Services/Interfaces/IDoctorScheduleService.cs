using MediCare.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorScheduleService
    {
        Task<ServiceResult<bool>> SaveScheduleAsync(
            Guid doctorUserId,
            SaveDoctorScheduleRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<CreateDoctorTimeOffResultDto>> AddTimeOffAsync(
            Guid doctorUserId,
            CreateDoctorTimeOffRequest request,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<bool>> RemoveTimeOffAsync(
            Guid doctorUserId,
            Guid timeOffId,
            CancellationToken cancellationToken = default);

        Task<ServiceResult<IReadOnlyList<AvailableSlotDto>>> GetAvailableSlotsAsync(
            Guid doctorProfileId,
            DateOnly localDate,
            CancellationToken cancellationToken = default);
    }
}
