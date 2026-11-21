using MediCare.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface ISpecializationRepository
    {
        Task<IReadOnlyList<Specialization>> GetActiveAsync(
            CancellationToken cancellationToken = default);
    }
}
