using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.DTOs.Auth;

public sealed class RegisterPatientRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;

    public DateOnly? DateOfBirth { get; init; }

    [StringLength(30)]
    public string? Gender { get; init; }

    [StringLength(200)]
    public string? Address { get; init; }

    [StringLength(120)]
    public string? EmergencyContact { get; init; }
}

public sealed class RegisterDoctorRequest
{
    [Required]
    [StringLength(160, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(30)]
    public string PhoneNumber { get; init; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Bio { get; init; }

    [Required]
    [StringLength(80)]
    public string City { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "99999999")]
    public decimal ConsultationFee { get; init; }

    [Range(5, 120)]
    public short SlotMinutes { get; init; } = 30;

    [Required]
    [MinLength(1)]
    public IReadOnlyCollection<Guid> SpecializationIds { get; init; }
        = Array.Empty<Guid>();
}

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(10)]
    [MaxLength(100)]
    public string NewPassword { get; init; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; init; } = string.Empty;
}

public sealed record AuthenticatedUserDto(
    Guid UserId,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    string? DoctorApprovalStatus);

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    AuthenticatedUserDto User);

public sealed record RegistrationResponse(
    Guid UserId,
    string Message);