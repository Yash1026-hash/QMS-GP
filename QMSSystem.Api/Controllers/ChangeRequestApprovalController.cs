using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChangeRequestApprovalController(UserDbContext database) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<OperatorChangeRequest>>> GetPendingChangeRequests()
    {
        var changeRequests = await database.OperatorChangeRequests
            .AsNoTracking()
            .Where(changeRequest => changeRequest.Status == "Pending")
            .OrderByDescending(changeRequest => changeRequest.RequestedDate)
            .ToListAsync();

        return Ok(changeRequests);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OperatorChangeRequest>> GetChangeRequest(int id)
    {
        var changeRequest = await database.OperatorChangeRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(request => request.Id == id);

        return changeRequest is null
            ? NotFound($"Change request with ID {id} was not found.")
            : Ok(changeRequest);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitDecision(
        [FromBody] OperatorChangeRequest request)
    {
        if (request.Id <= 0)
        {
            return BadRequest("Invalid change request ID.");
        }

        if (string.IsNullOrWhiteSpace(request.Decision))
        {
            return BadRequest("Decision is required.");
        }

        if (!request.Decision.Equals("accept", StringComparison.OrdinalIgnoreCase) &&
            !request.Decision.Equals("reject", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Decision must be either accept or reject.");
        }

        if (string.IsNullOrWhiteSpace(request.DecisionComment))
        {
            return BadRequest("Decision comment is required.");
        }

        if (request.DecisionComment.Length > 1000)
        {
            return BadRequest("Decision comment cannot exceed 1000 characters.");
        }

        var existingRequest = await database.OperatorChangeRequests
            .FirstOrDefaultAsync(changeRequest => changeRequest.Id == request.Id);
        if (existingRequest is null ||
            !string.Equals(existingRequest.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(
                "Change request was not found or has already been processed.");
        }

        var decision = request.Decision.ToLowerInvariant();
        existingRequest.Decision = decision;
        existingRequest.DecisionComment = request.DecisionComment.Trim();
        existingRequest.DecisionByUserId = request.DecisionByUserId;
        existingRequest.DecisionDate = DateTime.Now;
        existingRequest.Status = decision == "accept" ? "Approved" : "Rejected";

        await database.SaveChangesAsync();

        return Ok(new
        {
            message = "Change request decision submitted successfully.",
            changeRequestId = request.Id,
            decision,
            status = existingRequest.Status
        });
    }
}
