using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<PatientProfile?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
