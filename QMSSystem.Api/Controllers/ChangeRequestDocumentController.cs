using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChangeRequestDocumentController : ControllerBase
{
    private readonly UserDbContext _context;

    public ChangeRequestDocumentController(UserDbContext context)
    {
        _context = context;
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveDocuments()
    {
        var documents = await _context.DocumentCreations
            .Where(document => document.Status == "Active")
            .OrderBy(document => document.DocumentNumber)
            .ToListAsync();

        return Ok(documents);

    }
    
    [HttpGet("{documentId:int}/deviations")]
    public async Task<IActionResult> GetActiveDeviations(int documentId)
    {
        var activeDeviationIds = await _context.DeviationReportRequests
            .Where(report =>
                report.DocumentId == documentId &&
                report.Status == 1) // Active
            .Select(report => report.DeviationId)
            .Distinct()
            .ToListAsync();

        var deviations = await _context.DeviationRequests
            .Where(deviation =>
                deviation.DocumentId == documentId &&
                activeDeviationIds.Contains(deviation.Id))
            .Select(deviation => new
            {
                deviation.Id,
                deviation.Title
            })
            .ToListAsync();

        return Ok(deviations);
    }
}