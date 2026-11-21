using MediCare.Data.Entities;
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
    public sealed class SpecializationRepository : ISpecializationRepository
    {
        private readonly MediCareDbContext _dbContext;

        public SpecializationRepository(MediCareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Specialization>> GetActiveAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Specializations
                .AsNoTracking()
                .Where(specialization => specialization.IsActive)
                .OrderBy(specialization => specialization.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
