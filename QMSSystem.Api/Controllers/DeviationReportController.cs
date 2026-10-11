using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviationReportController : ControllerBase
{
    private readonly DeviationReportService _reportService;

    public DeviationReportController(
        DeviationReportService reportService)
    {
        _reportService = reportService;
    }

    // GET:
    // api/DeviationReport/deviation/5
    [HttpGet("deviation/{deviationId:int}")]
    public async Task<ActionResult<IEnumerable<DeviationReportDto>>> GetReports(
        int deviationId)
    {
        var reports = await _reportService.GetReportsAsync(deviationId);

        if (reports == null)
        {
            return NotFound(new
            {
                message = $"Deviation with ID {deviationId} was not found."
            });
        }

        return Ok(reports);
    }

    // GET:
    // api/DeviationReport/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviationReportDto>> GetReport(int id)
    {
        var report = await _reportService.GetReportAsync(id);

        if (report == null)
        {
            return NotFound(new
            {
                message = $"Deviation report with ID {id} was not found."
            });
        }

        return Ok(report);
    }

    // POST:
    // api/DeviationReport
    [HttpPost]
    public async Task<ActionResult<DeviationReportDto>> CreateReport(
        [FromBody] DeviationReportDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new
            {
                message = "Deviation report data is required."
            });
        }

        if (dto.DeviationId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid deviation ID is required."
            });
        }

        if (dto.DocumentId <= 0)
        {
            return BadRequest(new
            {
                message = "A valid document ID is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Summary))
        {
            return BadRequest(new
            {
                message = "Report summary is required."
            });
        }

        var result = await _reportService.CreateReportAsync(dto);

        return CreatedAtAction(
            nameof(GetReport),
            new { id = result.Id },
            result);
    }
}