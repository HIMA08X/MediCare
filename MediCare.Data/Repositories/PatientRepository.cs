using MediCare.Data.Entities;
using MediCare.Data.Persistence;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly MediCareDbContext _dbContext;

    public PatientRepository(MediCareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PatientProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PatientProfiles
            .FirstOrDefaultAsync(
                patient => patient.UserId == userId,
                cancellationToken);
    }
}