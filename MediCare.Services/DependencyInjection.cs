using MediCare.Services.Interfaces;
using MediCare.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediCare.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDoctorScheduleService, DoctorScheduleService>();
        services.AddScoped<IDoctorDirectoryService, DoctorDirectoryService>();

        return services;
    }
}