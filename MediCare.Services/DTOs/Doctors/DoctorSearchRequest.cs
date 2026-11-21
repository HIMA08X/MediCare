using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Services.DTOs.Doctors
{
    public sealed class DoctorSearchRequest
    {
        public string? Keyword { get; init; }
        public string? City { get; init; }
        public Guid? SpecializationId { get; init; }
        public DateOnly? AvailableOn { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
