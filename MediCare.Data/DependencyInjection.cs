using MediCare.Data.Persistence;
using MediCare.Data.Repositories;
using MediCare.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediCare.Data
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataServices(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("MDCDefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'MDCDefaultConnection' was not found.");

            services.AddDbContext<MediCareDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IClinicSettingsRepository, ClinicSettingsRepository>();
            services.AddScoped<ISpecializationRepository, SpecializationRepository>();

            return services;
        }
    }
}
