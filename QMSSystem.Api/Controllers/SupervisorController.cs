using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/supervisor")]
[Authorize(Policy = "SupervisorOnly")]
public class SupervisorController : ControllerBase
{
    private readonly UserDbContext _context;

    public SupervisorController(UserDbContext context)
    {
        _context = context;
    }

    // Get pending change request count
    [HttpGet("change-requests/count")]
    public async Task<IActionResult> GetPendingChangeRequestCount()
    {
        int count = await _context.OperatorChangeRequests
            .CountAsync(x => x.Status == "Pending");

        return Ok(count);
    }

    // Get pending deviation count
    [HttpGet("deviations/count")]
    public async Task<IActionResult> GetPendingDeviationCount()
    {
        int count = await _context.DeviationRequests
            .CountAsync(x => x.Status == 0);

        return Ok(count);
    }

    // Get pending deviation report count
    [HttpGet("deviation-reports/count")]
    public async Task<IActionResult> GetPendingDeviationReportCount()
    {
        int count = await _context.DeviationReportRequests
            .CountAsync(x => x.Status == 0);

        return Ok(count);
    }

    // Get pending document count
    [HttpGet("documents/count")]
    public async Task<IActionResult> GetPendingDocumentCount()
    {
        int count = await _context.DocumentCreations
            .CountAsync(x => x.Status == "Pending");

        return Ok(count);
    }

    // Get pending documents with pagination and optional document-number search
    [HttpGet("documents/pending")]
    public async Task<IActionResult> GetPendingDocuments(
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? documentNumber = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var reviewerIds = await GetReviewerIdsAsync(cancellationToken);
        var reviewerId = reviewerIds.Count == 1
            ? reviewerIds[0]
            : (int?)null;

        var pendingDocuments = _context.DocumentCreations
            .Where(document => document.Status == "Pending");

        if (!string.IsNullOrWhiteSpace(documentNumber))
        {
            var searchTerm = documentNumber.Trim();

            pendingDocuments = pendingDocuments.Where(document =>
                document.DocumentNumber.Contains(searchTerm));
        }

        var totalCount = await pendingDocuments
            .CountAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        page = Math.Min(page, Math.Max(totalPages, 1));

        var pendingPage = await pendingDocuments
            .OrderByDescending(document => document.CreationOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var pendingDecisionDate = DateTime.UtcNow;

        if (reviewerId.HasValue)
        {
            foreach (var pendingDocument in pendingPage)
            {
                pendingDocument.DecisionBy ??= reviewerId.Value.ToString();
                pendingDocument.DecisionDate ??= pendingDecisionDate;
                pendingDocument.DecisionStatus = 0;
            }

            if (pendingPage.Count > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        var documents = pendingPage
            .Select(document => new Document
            {
                Id = document.Id,
                DocumentNumber = document.DocumentNumber,
                Title = document.Title,
                Department = document.Department,
                DocumentVersion = document.DocumentVersion,
                Status = document.Status,
                FileName = document.FileName,
                ContentType = document.ContentType,
                CreatedBy = document.CreatedBy,
                CreationOn = document.CreationOn,
                Comment = document.Comment,
                DecisionBy = reviewerId,
                DecisionDate = pendingDecisionDate,
                DecisionStatus = document.DecisionStatus ?? 0,
                Decision = "Pending",
            })
            .ToList();

        return Ok(new
        {
            Documents = documents,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // Save supervisor decision for a document
    [HttpPost("documents/decision")]
    public async Task<IActionResult> SaveDocumentDecision(
        [FromBody] Document? decisionRequest,
        CancellationToken cancellationToken)
    {
        if (decisionRequest is null || decisionRequest.Id <= 0)
        {
            return BadRequest("A valid document ID is required.");
        }

        var decision = decisionRequest.Decision?
            .Trim()
            .ToLowerInvariant() switch
        {
            "approve" => "Approved",

            // Return the document to its creator for corrections.
            "revision" => "Returned for revision",

            "reject" => "Rejected",

            _ => null
        };

        if (decision is null)
        {
            return BadRequest(
                "Decision must be approve, revision, or reject.");
        }

        // Approved documents become active.
        // Revision and rejection decisions leave documents inactive.
        var documentStatus = decision == "Approved"
            ? "Active"
            : "Inactive";

        var decisionStatus = decision == "Approved" ? 1 : 2;

        if (string.IsNullOrWhiteSpace(decisionRequest.Comment) ||
            decisionRequest.Comment.Trim().Length > 1000)
        {
            return BadRequest(
                "Reviewer comments are required and must not exceed 1000 characters.");
        }

        var document = await _context.DocumentCreations
            .FirstOrDefaultAsync(
                item => item.Id == decisionRequest.Id,
                cancellationToken);

        if (document is null)
        {
            return NotFound("Document not found.");
        }

        if (document.Status != "Pending")
        {
            return Conflict(
                "Only pending documents can receive a decision.");
        }

        var reviewerIds = await GetReviewerIdsAsync(cancellationToken);

        if (reviewerIds.Count != 1)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The authenticated supervisor could not be uniquely matched to a user account.");
        }

        var decisionTime = DateTime.UtcNow;
        var comments = decisionRequest.Comment.Trim();

        document.Status = documentStatus;
        document.DecisionBy = reviewerIds[0].ToString();
        document.DecisionDate = decisionTime;
        document.DecisionStatus = decisionStatus;

        var reviewerNote = $"Reviewer decision ({decision}): {comments}";

        document.Comment = string.IsNullOrWhiteSpace(document.Comment)
            ? reviewerNote
            : $"{document.Comment}{Environment.NewLine}{Environment.NewLine}{reviewerNote}";

        await _context.SaveChangesAsync(cancellationToken);

        // Return the completed decision to the UI.
        return Ok(new Document
        {
            Id = document.Id,
            Status = document.Status,
            Decision = decision,
            DecisionBy = reviewerIds[0],
            DecisionDate = document.DecisionDate,
            DecisionStatus = document.DecisionStatus ?? 0
        });
    }

    // Identify the authenticated supervisor's user account.
    private async Task<List<int>> GetReviewerIdsAsync(
        CancellationToken cancellationToken)
    {
        var reviewerName = User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(reviewerName))
        {
            return [];
        }

        var reviewerIds = await _context.Users
            .AsNoTracking()
            .Where(user => user.FullName == reviewerName)
            .Select(user => user.UserId)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (reviewerIds.Count == 0)
        {
            reviewerIds = await _context.Users
                .AsNoTracking()
                .Where(user => user.Username == reviewerName)
                .Select(user => user.UserId)
                .Take(2)
                .ToListAsync(cancellationToken);
        }

        return reviewerIds;
    }
}