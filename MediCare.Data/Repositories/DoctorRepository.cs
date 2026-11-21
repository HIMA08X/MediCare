using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Persistence;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediCare.Data.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly MediCareDbContext _dbContext;

    public DoctorRepository(MediCareDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<DoctorProfile?> GetByIdAsync(
        Guid doctorProfileId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(
                doctor => doctor.Id == doctorProfileId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorAvailability>> GetAvailabilityForDayAsync(
        Guid doctorProfileId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.DoctorAvailabilities
            .AsNoTracking()
            .Where(x =>
                x.DoctorProfileId == doctorProfileId &&
                x.DayOfWeek == dayOfWeek &&
                x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> HasTimeOffOverlapAsync(
        Guid doctorProfileId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DoctorTimeOffs.AnyAsync(
            x => x.DoctorProfileId == doctorProfileId &&
                 x.StartUtc < endUtc &&
                 x.EndUtc > startUtc,
            cancellationToken);
    }
    public Task<DoctorProfile?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DoctorProfiles
            .FirstOrDefaultAsync(
                doctor => doctor.UserId == userId,
                cancellationToken);
    }
    public async Task ReplaceAvailabilityAsync(
        Guid doctorProfileId,
        IReadOnlyCollection<DoctorAvailability> availability,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.DoctorAvailabilities
            .Where(item => item.DoctorProfileId == doctorProfileId)
            .ToListAsync(cancellationToken);

        _dbContext.DoctorAvailabilities.RemoveRange(existing);

        foreach (var item in availability)
        {
            item.Id = Guid.NewGuid();
            item.DoctorProfileId = doctorProfileId;

            await _dbContext.DoctorAvailabilities.AddAsync(
                item,
                cancellationToken);
        }
    }

    public async Task AddTimeOffAsync(
        DoctorTimeOff timeOff,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.DoctorTimeOffs.AddAsync(
            timeOff,
            cancellationToken);
    }

    public async Task<bool> DeleteTimeOffAsync(
        Guid doctorProfileId,
        Guid timeOffId,
        CancellationToken cancellationToken = default)
    {
        var timeOff = await _dbContext.DoctorTimeOffs
            .FirstOrDefaultAsync(
                item => item.Id == timeOffId &&
                        item.DoctorProfileId == doctorProfileId,
                cancellationToken);

        if (timeOff is null)
        {
            return false;
        }

        _dbContext.DoctorTimeOffs.Remove(timeOff);
        return true;
    }
    public async Task<(IReadOnlyList<DoctorProfile> Items, int TotalCount)>
        SearchPublicAsync(
            string? keyword,
            string? city,
            Guid? specializationId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(doctor =>
                doctor.ApprovalStatus == DoctorApprovalStatus.Approved &&
                doctor.IsListed &&
                doctor.User.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();

            query = query.Where(doctor =>
                doctor.User.FullName.Contains(normalizedKeyword) ||
                doctor.Bio.Contains(normalizedKeyword));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim();

            query = query.Where(doctor =>
                doctor.City.Contains(normalizedCity));
        }

        if (specializationId.HasValue)
        {
            query = query.Where(doctor =>
                doctor.Specializations.Any(link =>
                    link.SpecializationId == specializationId.Value &&
                    link.Specialization.IsActive));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Include(doctor => doctor.User)
            .Include(doctor => doctor.Specializations)
                .ThenInclude(link => link.Specialization)
            .OrderBy(doctor => doctor.User.FullName)
            .ThenBy(doctor => doctor.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<DoctorProfile?> GetPublicByIdAsync(
        Guid doctorProfileId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(doctor =>
                doctor.Id == doctorProfileId &&
                doctor.ApprovalStatus == DoctorApprovalStatus.Approved &&
                doctor.IsListed &&
                doctor.User.IsActive)
            .Include(doctor => doctor.User)
            .Include(doctor => doctor.Specializations)
                .ThenInclude(link => link.Specialization)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<DoctorProfile>> SearchPublicCandidatesAsync(
        string? keyword,
        string? city,
        Guid? specializationId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.DoctorProfiles
            .AsNoTracking()
            .Where(doctor =>
                doctor.ApprovalStatus == DoctorApprovalStatus.Approved &&
                doctor.IsListed &&
                doctor.User.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalizedKeyword = keyword.Trim();

            query = query.Where(doctor =>
                doctor.User.FullName.Contains(normalizedKeyword) ||
                doctor.Bio.Contains(normalizedKeyword));
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim();

            query = query.Where(doctor =>
                doctor.City.Contains(normalizedCity));
        }

        if (specializationId.HasValue)
        {
            query = query.Where(doctor =>
                doctor.Specializations.Any(link =>
                    link.SpecializationId == specializationId.Value &&
                    link.Specialization.IsActive));
        }

        return await query
            .Include(doctor => doctor.User)
            .Include(doctor => doctor.Specializations)
                .ThenInclude(link => link.Specialization)
            .OrderBy(doctor => doctor.User.FullName)
            .ThenBy(doctor => doctor.Id)
            .ToListAsync(cancellationToken);
    }
}