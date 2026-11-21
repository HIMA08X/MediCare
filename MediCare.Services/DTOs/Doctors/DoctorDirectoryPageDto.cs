using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs.Doctors
{
    public sealed class DoctorDirectoryPageDto
    {
        public IReadOnlyList<DoctorCardDto> Items { get; init; }
            = Array.Empty<DoctorCardDto>();

        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
    }
}
