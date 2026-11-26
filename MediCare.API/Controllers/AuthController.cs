using System.Security.Claims;
using MediCare.Data.Enums;
using MediCare.Services.DTOs;
using MediCare.Services.DTOs.Auth;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/patient")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(RegistrationResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterPatient(
        RegisterPatientRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterPatientAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return MapFailure(result);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Data);
    }

    [HttpPost("register/doctor")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(RegistrationResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterDoctor(
        RegisterDoctorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterDoctorAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return MapFailure(result);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Data);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(
        typeof(LoginResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return MapFailure(result);
        }

        return Ok(result.Data);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var result = await _authService.ChangePasswordAsync(
            userId,
            request,
            cancellationToken);

        if (!result.Succeeded)
        {
            return MapFailure(result);
        }

        return NoContent();
    }

    private IActionResult MapFailure<T>(ServiceResult<T> result)
    {
        var response = new { message = result.Message };

        return result.Error switch
        {
            ServiceError.ValidationFailed => BadRequest(response),
            ServiceError.Conflict => Conflict(response),
            ServiceError.NotFound => NotFound(response),
            ServiceError.Unauthorized => Unauthorized(response),
            ServiceError.Forbidden => Forbid(),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                new { message = "An unexpected error occurred." })
        };
    }
}