using MediCare.API.Options;
using MediCare.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace MediCare.API.Extensions;

public static class IdentitySeedExtensions
{
    public static async Task SeedInitialAdminAsync(
        this WebApplication app)
    {
        var options = app.Configuration
            .GetSection("AdminSeed")
            .Get<AdminSeedOptions>() ?? new AdminSeedOptions();

        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.FullName) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Admin seed is enabled but its credentials are incomplete.");
        }

        using var scope = app.Services.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var email = options.Email.Trim();
        var adminRoleExists = await roleManager.RoleExistsAsync(
            MediCareRoles.Admin);

        if (!adminRoleExists)
        {
            var roleResult = await roleManager.CreateAsync(
                new IdentityRole<Guid>(MediCareRoles.Admin));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not create the Admin role: " +
                    string.Join(
                        "; ",
                        roleResult.Errors.Select(error => error.Description)));
            }
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            var alreadyAdmin = await userManager.IsInRoleAsync(
                existingUser,
                MediCareRoles.Admin);

            if (!alreadyAdmin)
            {
                throw new InvalidOperationException(
                    "The configured AdminSeed email belongs to an existing non-admin account. Choose a new seed email.");
            }

            return;
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = options.FullName.Trim(),
            IsActive = true,
            EmailConfirmed = true,
            LockoutEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(
            user,
            options.Password);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Could not create the initial Admin account: " +
                string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description)));
        }

        var addRoleResult = await userManager.AddToRoleAsync(
            user,
            MediCareRoles.Admin);

        if (!addRoleResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Could not assign the Admin role: " +
                string.Join(
                    "; ",
                    addRoleResult.Errors.Select(error => error.Description)));
        }
    }
}