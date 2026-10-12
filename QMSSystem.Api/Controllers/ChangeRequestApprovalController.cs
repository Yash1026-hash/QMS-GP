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
public class ChangeRequestApprovalController : ControllerBase
{
    private readonly UserDbContext _context;

    public ChangeRequestApprovalController(UserDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // GET: api/ChangeRequestApproval/pending
    // Get all pending change requests
    // =========================================================
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>> GetPendingChangeRequests()
    {
        var changeRequests = await _context.OperatorChangeRequests
            .Where(cr => cr.Status == "Pending")
            .OrderByDescending(cr => cr.RequestedDate)
            .ToListAsync();

        return Ok(changeRequests);
    }

    // =========================================================
    // GET: api/ChangeRequestApproval/closed
    // Get all closed / historical change requests
    // =========================================================
    [HttpGet("closed")]
    public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>> GetClosedChangeRequests()
    {
        var changeRequests = await _context.OperatorChangeRequests
            .Where(cr => cr.Status == "Closed" || cr.Status == "Approved" || cr.Status == "Rejected")
            .OrderByDescending(cr => cr.DecisionDate ?? cr.RequestedDate)
            .ToListAsync();

        return Ok(changeRequests);
    }

    // =========================================================
    // GET: api/ChangeRequestApproval/{id}
    // Get one change request by ID
    // =========================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OperatorChangeRequest>> GetChangeRequest(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid change request ID.");
        }

        var changeRequest = await _context.OperatorChangeRequests
            .FirstOrDefaultAsync(cr => cr.Id == id);

        if (changeRequest == null)
        {
            return NotFound($"Change request with ID {id} was not found.");
        }

        return Ok(changeRequest);
    }

    // =========================================================
    // POST: api/ChangeRequestApproval
    // Submit approval or rejection decision
    // =========================================================
    [HttpPost]
    public async Task<IActionResult> SubmitDecision([FromBody] OperatorChangeRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request data is required.");
        }

        if (request.Id <= 0)
        {
            return BadRequest("Invalid change request ID.");
        }

        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest("Decision is required.");
        }

        var decisionKey = request.Decision.Trim().ToLowerInvariant();
        bool isApproved = decisionKey == "accept" || decisionKey == "approved" || decisionKey == "approve";
        bool isRejected = decisionKey == "reject" || decisionKey == "rejected";

        if (!isApproved && !isRejected)
        {
            return BadRequest("Decision must be either Accept or Reject.");
        }

        if (string.IsNullOrWhiteSpace(request.DecisionComment))
        {
            return BadRequest("Decision comment is required.");
        }

        if (request.DecisionComment.Length > 1000)
        {
            return BadRequest("Decision comment cannot exceed 1000 characters.");
        }

        var existing = await _context.OperatorChangeRequests
            .FirstOrDefaultAsync(cr => cr.Id == request.Id && cr.Status == "Pending");

        if (existing == null)
        {
            return NotFound("Change request was not found or has already been processed.");
        }

        var outcome = isApproved ? "Approved" : "Rejected";
        existing.Decision = outcome;
        existing.Status = "Closed"; // Request is closed and moved to history
        existing.DecisionComment = request.DecisionComment.Trim();
        existing.DecisionByUserId = request.DecisionByUserId.HasValue && request.DecisionByUserId.Value > 0
            ? request.DecisionByUserId.Value
            : 1;
        existing.DecisionDate = DateTime.UtcNow;

        if (isApproved)
        {
            var doc = await _context.DocumentCreations.FirstOrDefaultAsync(d => d.Id == existing.DocumentId);
            if (doc != null)
            {
                // 1. Push older version to Inactive in DocumentHistories (accessible only to admin)
                var olderVersionHistory = new DocumentHistory
                {
                    DocumentId = doc.Id,
                    DocumentNumber = doc.DocumentNumber,
                    Title = doc.Title,
                    Department = doc.Department,
                    DocumentVersion = doc.DocumentVersion, // the previous version
                    Status = "Inactive", // pushed to inactive
                    FileName = doc.FileName,
                    ContentType = doc.ContentType,
                    FileData = doc.FileData,
                    CreatedBy = doc.CreatedBy,
                    CreationOn = doc.CreationOn,
                    Comment = $"Archived upon approval of Change Request #{existing.Id} ({existing.ChangeType}): {existing.Title}",
                    ArchivedOn = DateTime.UtcNow,
                    ArchivedBy = existing.DecisionByUserId ?? 1
                };
                _context.DocumentHistories.Add(olderVersionHistory);

                // 2. Increment document version and keep active
                doc.DocumentVersion = doc.DocumentVersion + 1;
                doc.Status = "Active"; // new version is active
                doc.Comment = $"Updated via approved Change Request #{existing.Id}: {existing.Title}";
                doc.DecisionBy = existing.DecisionByUserId?.ToString();
                doc.DecisionDate = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = $"Change request #{existing.Id} successfully {outcome.ToLowerInvariant()} and marked as Closed.",
            changeRequestId = existing.Id,
            decision = outcome,
            status = "Closed"
        });
    }
}