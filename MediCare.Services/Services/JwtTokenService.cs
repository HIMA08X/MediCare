using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediCare.Data.Entities;
using MediCare.Services.Interfaces;
using MediCare.Services.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MediCare.Services.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private const string DoctorApprovedClaim = "doctor_approved";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string CreateToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        bool? isDoctorApproved,
        DateTime expiresAtUtc)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(role =>
            new Claim(ClaimTypes.Role, role)));

        if (roles.Contains(MediCareRoles.Doctor) &&
            isDoctorApproved.HasValue)
        {
            claims.Add(new Claim(
                DoctorApprovedClaim,
                isDoctorApproved.Value ? "true" : "false"));
        }

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}