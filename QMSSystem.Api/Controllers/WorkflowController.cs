using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services.Workflow;
using QMSSystem.Shared.Dtos.Workflow;

namespace QMSSystem.Api.Controllers;

// Endpoints that connect the module pages. The Web pages call these to fill
// their dropdowns and to show the traceability timeline.
[ApiController]
[Route("api/workflow")]
public sealed class WorkflowController(WorkflowQueries queries, WorkflowService workflow) : ControllerBase
{
    // Operator home: approved documents with a "Raise Deviation" button.
    [HttpGet("documents/active")]
    public async Task<ActionResult<List<ActiveDocumentDto>>> GetActiveDocuments() =>
        await queries.GetActiveDocumentsAsync();

    // /Deviations/SubmitReport picker.
    [HttpGet("deviations/ready-for-report")]
    public async Task<ActionResult<List<DeviationReadyForReportDto>>> GetDeviationsReadyForReport(
        [FromQuery] int? reportedByUserId) =>
        await queries.GetDeviationsReadyForReportAsync(reportedByUserId);

    // /ChangeRequests/Edit picker.
    [HttpGet("reports/ready-for-change-request")]
    public async Task<ActionResult<List<ReportReadyForChangeRequestDto>>> GetReportsReadyForChangeRequest() =>
        await queries.GetReportsReadyForChangeRequestAsync();

    // /Documents/UploadRevision picker.
    [HttpGet("documents/{documentId:int}/change-requests-ready-for-revision")]
    public async Task<ActionResult<List<ChangeRequestReadyForRevisionDto>>> GetChangeRequestsReadyForRevision(
        int documentId) =>
        await queries.GetChangeRequestsReadyForRevisionAsync(documentId);

    // /Deviations/Details traceability timeline.
    [HttpGet("deviations/{deviationId:int}/timeline")]
    public async Task<ActionResult<List<TimelineEntryDto>>> GetTimeline(int deviationId)
    {
        var timeline = await queries.GetTimelineAsync(deviationId);
        return timeline is null ? NotFound() : timeline;
    }

    // Supervisor closes a deviation by hand. The Web page must check the Supervisor role.
    [HttpPost("deviations/{deviationId:int}/close")]
    public async Task<IActionResult> CloseDeviation(int deviationId, [FromBody] CloseDeviationRequest request)
    {
        await workflow.CloseDeviationAsync(deviationId, request.ActorUserId, request.Reason);
        return NoContent();
    }
}

public sealed record CloseDeviationRequest(int ActorUserId, string Reason);
