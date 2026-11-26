using MediCare.Services.DTOs.Doctors;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/specializations")]
public sealed class SpecializationsController : ControllerBase
{
    private readonly IDoctorDirectoryService _directoryService;

    public SpecializationsController(
        IDoctorDirectoryService directoryService)
    {
        _directoryService = directoryService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<SpecializationDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SpecializationDto>>> GetActive(
        CancellationToken cancellationToken)
    {
        var specializations =
            await _directoryService.GetSpecializationsAsync(cancellationToken);

        return Ok(specializations);
    }
}