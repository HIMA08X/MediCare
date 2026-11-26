using MediCare.Data.Entities;

namespace MediCare.Services.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        bool? isDoctorApproved,
        DateTime expiresAtUtc);
}