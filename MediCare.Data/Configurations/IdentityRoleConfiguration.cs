using MediCare.Data.Constants;
using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediCare.Data.Configurations;

public sealed class IdentityRoleConfiguration
    : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = MediCareRoles.Patient,
                NormalizedName = MediCareRoles.Patient.ToUpperInvariant()
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = MediCareRoles.Doctor,
                NormalizedName = MediCareRoles.Doctor.ToUpperInvariant()
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = MediCareRoles.Admin,
                NormalizedName = MediCareRoles.Admin.ToUpperInvariant()
            });
    }
}