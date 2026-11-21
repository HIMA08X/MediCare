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

    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly MediCareDbContext _dbContext;

        public UnitOfWork(
            MediCareDbContext dbContext,
            IAppointmentRepository appointments,
            IDoctorRepository doctors,
            IPatientRepository patients,
            IClinicSettingsRepository clinicSettings)
        {
            _dbContext = dbContext;
            Appointments = appointments;
            Doctors = doctors;
            Patients = patients;
            ClinicSettings = clinicSettings;
        }

        public IAppointmentRepository Appointments { get; }
        public IDoctorRepository Doctors { get; }
        public IPatientRepository Patients { get; }
        public IClinicSettingsRepository ClinicSettings { get; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(
                        cancellationToken);

                var result = await operation(cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result;
            });
        }
        public async Task AcquireDoctorLockAsync(
               Guid doctorProfileId,
               CancellationToken cancellationToken = default)
        {
            var resource = $"appointment-doctor:{doctorProfileId}";

            await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                $"""
                   DECLARE @result int;
                   EXEC @result = sys.sp_getapplock
                       @Resource = {resource},
                       @LockMode = 'Exclusive',
                       @LockOwner = 'Transaction',
                       @LockTimeout = 10000;
                   IF @result < 0
                       THROW 51000, 'Could not acquire doctor booking lock.', 1;
                   """,
                cancellationToken);
        }
        public async Task AddNotificationAsync(
               Notification notification,
    CancellationToken cancellationToken = default)
        {
            await _dbContext.Notifications.AddAsync(
                notification,
                cancellationToken);
        }
    }
}
