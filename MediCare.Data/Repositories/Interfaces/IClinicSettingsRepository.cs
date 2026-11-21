using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IClinicSettingsRepository
    {
        Task<IReadOnlyDictionary<string, string>> GetAllAsync(
            CancellationToken cancellationToken = default);
    }
}
