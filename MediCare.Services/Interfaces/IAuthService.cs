using MediCare.Services.DTOs;
using MediCare.Services.DTOs.Auth;

namespace MediCare.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<RegistrationResponse>> RegisterPatientAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<RegistrationResponse>> RegisterDoctorAsync(
        RegisterDoctorRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);

    Task<ServiceResult<bool>> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
