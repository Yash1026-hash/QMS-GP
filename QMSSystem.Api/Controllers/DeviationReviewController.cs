using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Route("api/deviation-review")]
[Authorize(Policy = "SupervisorOnly")]
public class DeviationReviewController : ControllerBase
{
    private readonly DeviationReviewService _service;

    public DeviationReviewController(DeviationReviewService service)
    {
        _service = service;
    }

    // GET: api/deviation-review/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetDeviation(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid deviation ID.");
        }

        var deviation = await _service.GetDeviationByIdAsync(id);

        if (deviation == null)
        {
            return NotFound(
                $"Deviation with ID {id} was not found.");
        }

        return Ok(deviation);
    }

    // POST: api/deviation-review/5
    [HttpPost("{id:int}")]
    public async Task<IActionResult> SubmitDecision(
        int id,
        [FromBody] DeviationApproval approval)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid deviation ID.");
        }

        if (approval == null)
        {
            return BadRequest("Approval data is required.");
        }

        if (approval.Decision != 1 && approval.Decision != 2)
        {
            return BadRequest(
                "Decision must be either 1 (Approve) or 2 (Reject).");
        }

        if (approval.Decision == 2 &&
            string.IsNullOrWhiteSpace(approval.DecisionComments))
        {
            return BadRequest(
                "Comments are required when rejecting a deviation.");
        }

        var decisionBy = User.Identity?.Name;

        if (string.IsNullOrWhiteSpace(decisionBy))
        {
            decisionBy = "Supervisor";
        }

        var success = await _service.SubmitDecisionAsync(
            id,
            approval.Decision,
            approval.DecisionComments,
            decisionBy);

        if (!success)
        {
            return NotFound(
                "Deviation was not found or has already been processed.");
        }

        return Ok(new
        {
            message = "Deviation decision submitted successfully.",
            deviationId = id,
            decision = approval.Decision
        });
    }
}