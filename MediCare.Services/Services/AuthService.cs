using MediCare.Data.Entities;
using MediCare.Data.Enums;
using MediCare.Data.Persistence;
using MediCare.Services.DTOs;
using MediCare.Services.DTOs.Auth;
using MediCare.Services.Interfaces;
using MediCare.Services.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

namespace MediCare.Services.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MediCareDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        MediCareDbContext dbContext,
        IJwtTokenService jwtTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<ServiceResult<RegistrationResponse>> RegisterPatientAsync(
        RegisterPatientRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        var emailExists = await _userManager.FindByEmailAsync(email);

        if (emailExists is not null)
        {
            return Failure<RegistrationResponse>(
                ServiceError.Conflict,
                "An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            FullName = request.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = false,
            LockoutEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await ExecuteRegistrationTransactionAsync(
            async () =>
            {
                var createResult = await _userManager.CreateAsync(
                    user,
                    request.Password);

                if (!createResult.Succeeded)
                {
                    return Failure<RegistrationResponse>(
                        ServiceError.ValidationFailed,
                        FormatIdentityErrors(createResult));
                }

                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    MediCareRoles.Patient);

                if (!roleResult.Succeeded)
                {
                    return Failure<RegistrationResponse>(
                        ServiceError.ValidationFailed,
                        FormatIdentityErrors(roleResult));
                }

                var profile = new PatientProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DateOfBirth = request.DateOfBirth,
                    Gender = NormalizeOptional(request.Gender),
                    Address = NormalizeOptional(request.Address),
                    EmergencyContact =
                        NormalizeOptional(request.EmergencyContact)
                };

                await _dbContext.PatientProfiles.AddAsync(
                    profile,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return ServiceResult<RegistrationResponse>.Success(
                    new RegistrationResponse(
                        user.Id,
                        "Patient account was created."));
            },
            cancellationToken);
    }

    public async Task<ServiceResult<RegistrationResponse>> RegisterDoctorAsync(
        RegisterDoctorRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var licenseNumber = request.LicenseNumber.Trim();

        var emailExists = await _userManager.FindByEmailAsync(email);

        if (emailExists is not null)
        {
            return Failure<RegistrationResponse>(
                ServiceError.Conflict,
                "An account with this email already exists.");
        }

        var licenseExists = await _dbContext.DoctorProfiles
            .AnyAsync(
                doctor => doctor.LicenseNumber == licenseNumber,
                cancellationToken);

        if (licenseExists)
        {
            return Failure<RegistrationResponse>(
                ServiceError.Conflict,
                "This medical license is already registered.");
        }

        var specializationIds = request.SpecializationIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        if (specializationIds.Length == 0)
        {
            return Failure<RegistrationResponse>(
                ServiceError.ValidationFailed,
                "At least one valid specialization is required.");
        }

        var activeSpecializationIds = await _dbContext.Specializations
            .Where(specialization =>
                specialization.IsActive &&
                specializationIds.Contains(specialization.Id))
            .Select(specialization => specialization.Id)
            .ToListAsync(cancellationToken);

        if (activeSpecializationIds.Count != specializationIds.Length)
        {
            return Failure<RegistrationResponse>(
                ServiceError.ValidationFailed,
                "One or more selected specializations are unavailable.");
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            PhoneNumber = request.PhoneNumber.Trim(),
            FullName = request.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = false,
            LockoutEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        return await ExecuteRegistrationTransactionAsync(
            async () =>
            {
                var createResult = await _userManager.CreateAsync(
                    user,
                    request.Password);

                if (!createResult.Succeeded)
                {
                    return Failure<RegistrationResponse>(
                        ServiceError.ValidationFailed,
                        FormatIdentityErrors(createResult));
                }

                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    MediCareRoles.Doctor);

                if (!roleResult.Succeeded)
                {
                    return Failure<RegistrationResponse>(
                        ServiceError.ValidationFailed,
                        FormatIdentityErrors(roleResult));
                }

                var doctor = new DoctorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    LicenseNumber = licenseNumber,
                    Bio = request.Bio?.Trim() ?? string.Empty,
                    City = request.City.Trim(),
                    ConsultationFee = request.ConsultationFee,
                    SlotMinutes = request.SlotMinutes,
                    ApprovalStatus = DoctorApprovalStatus.PendingApproval,
                    IsListed = false
                };

                await _dbContext.DoctorProfiles.AddAsync(
                    doctor,
                    cancellationToken);

                var doctorSpecializations = activeSpecializationIds
                    .Select(specializationId => new DoctorSpecialization
                    {
                        DoctorProfileId = doctor.Id,
                        SpecializationId = specializationId
                    })
                    .ToArray();

                await _dbContext.DoctorSpecializations.AddRangeAsync(
                    doctorSpecializations,
                    cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return ServiceResult<RegistrationResponse>.Success(
                    new RegistrationResponse(
                        user.Id,
                        "Doctor account was created and is awaiting approval."));
            },
            cancellationToken);
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Failure<LoginResponse>(
                ServiceError.Unauthorized,
                "Email or password is incorrect.");
        }

        if (!user.IsActive)
        {
            return Failure<LoginResponse>(
                ServiceError.Forbidden,
                "This account is inactive.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Failure<LoginResponse>(
                ServiceError.Forbidden,
                "This account is temporarily locked. Try again later.");
        }

        var passwordIsValid = await _userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordIsValid)
        {
            await _userManager.AccessFailedAsync(user);

            return Failure<LoginResponse>(
                ServiceError.Forbidden,
                "Email or password is incorrect.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var roleList = roles.ToArray();

        bool? isDoctorApproved = null;
        string? doctorApprovalStatus = null;

        if (roleList.Contains(MediCareRoles.Doctor))
        {
            var doctor = await _dbContext.DoctorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    profile => profile.UserId == user.Id,
                    cancellationToken);

            if (doctor is null)
            {
                return Failure<LoginResponse>(
                    ServiceError.Forbidden,
                    "Doctor profile was not found.");
            }

            isDoctorApproved =
                doctor.ApprovalStatus == DoctorApprovalStatus.Approved &&
                doctor.IsListed;

            doctorApprovalStatus = doctor.ApprovalStatus.ToString();
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);

        var accessToken = _jwtTokenService.CreateToken(
            user,
            roleList,
            isDoctorApproved,
            expiresAtUtc);

        var authenticatedUser = new AuthenticatedUserDto(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            roleList,
            doctorApprovalStatus);

        return ServiceResult<LoginResponse>.Success(
            new LoginResponse(
                accessToken,
                expiresAtUtc,
                authenticatedUser));
    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return Failure<bool>(
                ServiceError.NotFound,
                "User account was not found.");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            return Failure<bool>(
                ServiceError.ValidationFailed,
                FormatIdentityErrors(result));
        }

        return ServiceResult<bool>.Success(true);
    }

    private async Task<ServiceResult<T>>
        ExecuteRegistrationTransactionAsync<T>(
           Func<Task<ServiceResult<T>>> operation,
           CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction =
                    await _dbContext.Database.BeginTransactionAsync(
                        System.Data.IsolationLevel.ReadCommitted,
                        cancellationToken);

                try
                {
                    var result = await operation();

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _dbContext.ChangeTracker.Clear();
                        return result;
                    }

                    await transaction.CommitAsync(cancellationToken);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();
                    throw;
                }
            });
        }
        catch (DbUpdateException exception)
            when (IsUniqueViolation(exception))
        {
            _dbContext.ChangeTracker.Clear();

            return ServiceResult<T>.Failure(
                ServiceError.Conflict,
                "An account with the same email or license number already exists.");
        }
    }

    private static bool IsUniqueViolation(Exception exception)
    {
        while (exception is not null)
        {
            if (exception is SqlException sqlException &&
                sqlException.Number is 2601 or 2627)
            {
                return true;
            }

            exception = exception.InnerException!;
        }

        return false;
    }

    private static string NormalizeEmail(string email) =>
        email.Trim();

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string FormatIdentityErrors(IdentityResult result) =>
        string.Join(
            " ",
            result.Errors.Select(error => error.Description));

    private static ServiceResult<T> Failure<T>(
        ServiceError error,
        string message) =>
        ServiceResult<T>.Failure(error, message);
}