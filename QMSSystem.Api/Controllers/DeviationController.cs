
using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviationController : ControllerBase
{
    private readonly DeviationService _deviationService;

    public DeviationController(DeviationService deviationService)
    {
        _deviationService = deviationService;
    }

    // GET: api/Deviation
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviationRequestDto>>> GetDeviations()
    {
        var deviations = await _deviationService.GetDeviationsAsync();
        return Ok(deviations);
    }

    // GET: api/Deviation/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviationRequestDto>> GetDeviation(int id)
    {
        var deviation = await _deviationService.GetDeviationAsync(id);

        if (deviation == null)
        {
            return NotFound(new
            {
                message = $"Deviation with ID {id} was not found."
            });
        }

        return Ok(deviation);
    }

    // POST: api/Deviation
    [HttpPost]
    public async Task<ActionResult<DeviationRequestDto>> CreateDeviation(
        [FromBody] DeviationRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return BadRequest(new
            {
                message = "Deviation title is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new
            {
                message = "Deviation description is required."
            });
        }

        if (dto.DocumentId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid document ID is required."
            });
        }

        if (dto.Priority is not ("Minor" or "Major" or "Critical"))
        {
            return BadRequest(new
            {
                message = "Select a valid priority."
            });
        }

        var result = await _deviationService.CreateDeviationAsync(dto);

        return CreatedAtAction(
            nameof(GetDeviation),
            new { id = result.Id },
            result);
    }
}
