using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviationReportsController : ControllerBase
{
    private readonly UserDbContext _context;

    public DeviationReportsController(UserDbContext context)
    {
        _context = context;
    }

    // GET: api/DeviationReports
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviationReportRequest>>> GetAll()
    {
        var reports = await _context.DeviationReportRequests
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        return Ok(reports);
    }

    // GET: api/DeviationReports/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviationReportRequest>> GetById(int id)
    {
        var report = await _context.DeviationReportRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (report == null)
        {
            return NotFound(new
            {
                message = $"Deviation report {id} was not found."
            });
        }

        return Ok(report);
    }

    // GET: api/DeviationReports/deviation/10
    [HttpGet("deviation/{deviationId:int}")]
    public async Task<ActionResult<DeviationReportRequest>>
        GetByDeviationId(int deviationId)
    {
        var report = await _context.DeviationReportRequests
            .AsNoTracking()
            .Where(x => x.DeviationId == deviationId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync();

        if (report == null)
        {
            return NotFound(new
            {
                message =
                    $"No report found for deviation {deviationId}."
            });
        }

        return Ok(report);
    }

    // POST: api/DeviationReports
    [HttpPost]
    public async Task<ActionResult<DeviationReportRequest>>
        Create(DeviationReportRequest request)
    {
        if (request == null)
        {
            return BadRequest(new
            {
                message = "Deviation report is required."
            });
        }

        request.Id = 0;
        request.CreatedDate = DateTime.UtcNow;
        request.Status = 0;

        _context.DeviationReportRequests.Add(request);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = request.Id },
            request);
    }
}