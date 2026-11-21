using MediCare.Data.Persistence;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public sealed class ClinicSettingsRepository : IClinicSettingsRepository
    {
        private readonly MediCareDbContext _dbContext;

        public ClinicSettingsRepository(MediCareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.ClinicSettings
                .AsNoTracking()
                .ToDictionaryAsync(
                    setting => setting.Key,
                    setting => setting.Value,
                    cancellationToken);
        }
    }
}
