using MediCare.Data.Enums;
using MediCare.Services.DTOs;
using MediCare.Services.DTOs.Doctors;
using MediCare.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MediCare.API.Controllers;

[ApiController]
[Route("api/doctors")]
public sealed class DoctorsController : ControllerBase
{
    private readonly IDoctorDirectoryService _directoryService;
    private readonly IDoctorScheduleService _scheduleService;

    public DoctorsController(
        IDoctorDirectoryService directoryService,
        IDoctorScheduleService scheduleService)
    {
        _directoryService = directoryService;
        _scheduleService = scheduleService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DoctorDirectoryPageDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorDirectoryPageDto>> Search(
        [FromQuery] DoctorSearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _directoryService.SearchAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{doctorProfileId:guid}")]
    [ProducesResponseType(typeof(DoctorCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorCardDto>> GetDetails(
        Guid doctorProfileId,
        CancellationToken cancellationToken)
    {
        var doctor = await _directoryService.GetDetailsAsync(
            doctorProfileId,
            cancellationToken);

        if (doctor is null)
        {
            return NotFound();
        }

        return Ok(doctor);
    }

    [HttpGet("{doctorProfileId:guid}/available-slots")]
    [ProducesResponseType(
        typeof(IReadOnlyList<AvailableSlotDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<AvailableSlotDto>>> GetAvailableSlots(
        Guid doctorProfileId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (date is null)
        {
            return BadRequest(new
            {
                message = "Query parameter 'date' is required. Use yyyy-MM-dd."
            });
        }

        var result = await _scheduleService.GetAvailableSlotsAsync(
            doctorProfileId,
            date.Value,
            cancellationToken);

        if (!result.Succeeded)
        {
            return result.Error switch
            {
                ServiceError.NotFound => NotFound(new { result.Message }),
                ServiceError.Conflict => Conflict(new { result.Message }),
                ServiceError.Forbidden => Forbid(),
                _ => BadRequest(new { result.Message })
            };
        }

        return Ok(result.Data);
    }
}