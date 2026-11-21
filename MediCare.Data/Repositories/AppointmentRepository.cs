using MediCare.Data.Entities;
using MediCare.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using MediCare.Data.Enums;
using MediCare.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data.Repositories
{
    public sealed class AppointmentRepository : IAppointmentRepository
    {
        private readonly MediCareDbContext _dbContext;

        public AppointmentRepository(MediCareDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> HasOverlapAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Appointments.AnyAsync(
                appointment =>
                    appointment.DoctorProfileId == doctorProfileId &&
                    (appointment.Status == AppointmentStatus.Pending ||
                     appointment.Status == AppointmentStatus.Confirmed) &&
                    appointment.StartUtc < endUtc &&
                    appointment.EndUtc > startUtc,
                cancellationToken);
        }

        public async Task<Appointment?> GetByIdAsync(
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .Include(appointment => appointment.Patient)
                .Include(appointment => appointment.Doctor)
                .FirstOrDefaultAsync(
                    appointment => appointment.Id == appointmentId,
                    cancellationToken);
        }

        public async Task AddAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default)
        {
            await _dbContext.Appointments.AddAsync(
                appointment,
                cancellationToken);
        }

        public Task<bool> HasOverlapExceptAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            Guid excludedAppointmentId,
            CancellationToken cancellationToken = default)
        {
            return _dbContext.Appointments.AnyAsync(
                appointment =>
                    appointment.DoctorProfileId == doctorProfileId &&
                    appointment.Id != excludedAppointmentId &&
                    (appointment.Status == AppointmentStatus.Pending ||
                     appointment.Status == AppointmentStatus.Confirmed) &&
                    appointment.StartUtc < endUtc &&
                    appointment.EndUtc > startUtc,
                cancellationToken);
        }
    public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetForPatientAsync(
            Guid patientProfileId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Appointments
                .AsNoTracking()
                .Where(appointment =>
                    appointment.PatientProfileId == patientProfileId)
                .Include(appointment => appointment.Doctor)
                    .ThenInclude(doctor => doctor.User)
                .Include(appointment => appointment.Patient)
                    .ThenInclude(patient => patient.User)
                .OrderByDescending(appointment => appointment.StartUtc);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(IReadOnlyList<Appointment> Items, int TotalCount)> GetForDoctorAsync(
            Guid doctorProfileId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Appointments
                .AsNoTracking()
                .Where(appointment =>
                    appointment.DoctorProfileId == doctorProfileId)
                .Include(appointment => appointment.Doctor)
                    .ThenInclude(doctor => doctor.User)
                .Include(appointment => appointment.Patient)
                    .ThenInclude(patient => patient.User)
                .OrderBy(appointment => appointment.StartUtc);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
        public async Task<IReadOnlyList<Appointment>> GetOverlappingActiveAsync(
            Guid doctorProfileId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.Appointments
                .AsNoTracking()
                .Where(appointment =>
                    appointment.DoctorProfileId == doctorProfileId &&
                    (appointment.Status == AppointmentStatus.Pending ||
                     appointment.Status == AppointmentStatus.Confirmed) &&
                    appointment.StartUtc < endUtc &&
                    appointment.EndUtc > startUtc)
                .Include(appointment => appointment.Patient)
                    .ThenInclude(patient => patient.User)
                .OrderBy(appointment => appointment.StartUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
