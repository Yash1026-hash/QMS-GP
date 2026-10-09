using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

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
            .OrderBy(document => document.Id)
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
    [HttpPost("FormSubmission")]
    public async Task<ActionResult<int>> FormSubmission(
        [FromBody] ChangeRequestForm formModel)
    {
        OperatorChangeRequest OCR = new OperatorChangeRequest
        {
            DocumentId = formModel.DocumentId,
            Title = formModel.Title,
            ChangeType = formModel.ChangeType,
            Description = formModel.Description,
            RequestedByUserId = formModel.RequestedByUserId,
            RequestedDate = formModel.RequestedDate,
            Status = "Pending",
        };

        _context.OperatorChangeRequests.Add(OCR);
        await _context.SaveChangesAsync();

        foreach (var deviationId in formModel.SelectedDeviationIds)
        {
            var mapping = new OperatorChangeRequestDeviation
            {
                ChangeRequestId = OCR.Id,
                DeviationId = deviationId
            };

            _context.OperatorChangeRequestDeviations.Add(mapping);
        }

        if (formModel.SelectedDeviationIds.Count > 0)
        {
            await _context.SaveChangesAsync();
        }

        return Ok(OCR.Id);
    }
}