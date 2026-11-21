using MediCare.Data.Entities;
using MediCare.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IAppointmentRepository Appointments { get; }

        Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default);

        Task AcquireDoctorLockAsync(
            Guid doctorProfileId,
            CancellationToken cancellationToken = default);

        Task AddNotificationAsync(
           Notification notification,
           CancellationToken cancellationToken = default);

        IDoctorRepository Doctors { get; }
        IPatientRepository Patients { get; }
        IClinicSettingsRepository ClinicSettings { get; }

    }
}
