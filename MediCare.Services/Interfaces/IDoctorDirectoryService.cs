using MediCare.Services.DTOs.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.Interfaces
{
    public interface IDoctorDirectoryService
    {
        Task<DoctorDirectoryPageDto> SearchAsync(
            DoctorSearchRequest request,
            CancellationToken cancellationToken = default);

        Task<DoctorCardDto?> GetDetailsAsync(
            Guid doctorProfileId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SpecializationDto>> GetSpecializationsAsync(
            CancellationToken cancellationToken = default);
    }
}
