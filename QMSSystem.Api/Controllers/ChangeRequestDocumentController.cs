using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
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
            .Where(document => document.Status == "Active" || document.Status == "Approved")
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
        if (formModel.SelectedDeviationIds is not { Count: > 0 })
        {
            return BadRequest(new
            {
                message = "At least one associated deviation must be selected."
            });
        }

        var selectedDeviationIds = formModel.SelectedDeviationIds
            .Distinct()
            .ToList();
        var eligibleDeviationIds = await _context.DeviationRequests
            .Where(deviation =>
                deviation.DocumentId == formModel.DocumentId &&
                selectedDeviationIds.Contains(deviation.Id) &&
                _context.DeviationReportRequests.Any(report =>
                    report.DocumentId == formModel.DocumentId &&
                    report.DeviationId == deviation.Id &&
                    report.Status == 1))
            .Select(deviation => deviation.Id)
            .ToListAsync();

        if (eligibleDeviationIds.Count != selectedDeviationIds.Count)
        {
            return BadRequest(new
            {
                message = "One or more selected deviations are not eligible for this document."
            });
        }

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

        foreach (var deviationId in selectedDeviationIds)
        {
            var mapping = new OperatorChangeRequestDeviation
            {
                ChangeRequestId = OCR.Id,
                DeviationId = deviationId
            };

            _context.OperatorChangeRequestDeviations.Add(mapping);
        }

        await _context.SaveChangesAsync();

        return Ok(OCR.Id);
    }
}