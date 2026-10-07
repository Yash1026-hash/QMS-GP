using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Services.Workflow;
using QMSSystem.Shared.Enums;
using QMSSystem.Shared.Workflow;
using static QMSSystem.Tests.TestDatabase;

namespace QMSSystem.Tests;

// The full journey across the three modules, step by step.
public sealed class WorkflowChainTests : IDisposable
{
    private readonly TestDatabase db = new();

    public void Dispose() => db.Dispose();

    [Fact]
    public async Task Full_chain_closes_the_deviation_when_the_new_revision_is_approved()
    {
        // 1. SOP created and approved: it becomes an active document.
        var firstVersion = await db.Modules.CreateDocumentAsync("SOP-014", Ravi);
        await db.Modules.ApproveRevisionAsync(firstVersion.Id, Suresh);
        var documentId = firstVersion.DocumentId;
        Assert.Contains(await db.Queries.GetActiveDocumentsAsync(), document => document.Id == documentId);

        // 2. Deviation raised and accepted: it appears in the Submit Report picker.
        var deviation = await db.Modules.RaiseDeviationAsync(documentId, Ravi);
        await db.Modules.ReviewDeviationAsync(deviation.Id, Suresh, accept: true);
        var readyForReport = Assert.Single(await db.Queries.GetDeviationsReadyForReportAsync(Ravi));
        Assert.Equal(1, readyForReport.NextAttemptNumber);

        // 3. First report rejected, second report accepted with "change required".
        var firstReport = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);
        await db.Modules.ReviewReportAsync(firstReport.Id, Suresh, accept: false);
        Assert.Equal(2, Assert.Single(await db.Queries.GetDeviationsReadyForReportAsync()).NextAttemptNumber);
        var secondReport = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);
        Assert.Equal(2, secondReport.AttemptNumber);
        await db.Modules.ReviewReportAsync(secondReport.Id, Suresh, accept: true, changeRequired: true);
        Assert.Empty(await db.Queries.GetDeviationsReadyForReportAsync());
        Assert.Contains(await db.Queries.GetReportsReadyForChangeRequestAsync(), report => report.ReportId == secondReport.Id);

        // 4. Change request raised and submitted: the deviation waits for the change.
        var changeRequest = await db.Modules.RaiseChangeRequestAsync(secondReport.Id, Ravi);
        Assert.Equal(documentId, changeRequest.DocumentId);
        Assert.Equal(deviation.Id, changeRequest.DeviationId);
        Assert.Empty(await db.Queries.GetReportsReadyForChangeRequestAsync());
        await db.Modules.SubmitChangeRequestAsync(changeRequest.Id, Ravi);
        Assert.Equal(DeviationStatus.AwaitingChange, await StatusOf(deviation.Id));

        // 5. Change request approved: it appears in the Upload Revision picker of that SOP.
        await db.Modules.DecideChangeRequestAsync(changeRequest.Id, Suresh, approve: true);
        Assert.Contains(await db.Queries.GetChangeRequestsReadyForRevisionAsync(documentId), item => item.ChangeRequestId == changeRequest.Id);

        // 6. New revision uploaded and approved by the second supervisor.
        var secondVersion = await db.Modules.UploadRevisionAsync(documentId, changeRequest.Id, Priya);
        Assert.Equal(2, secondVersion.Version);
        Assert.Empty(await db.Queries.GetChangeRequestsReadyForRevisionAsync(documentId));
        Assert.Equal(DeviationStatus.AwaitingChange, await StatusOf(deviation.Id));
        await db.Modules.ApproveRevisionAsync(secondVersion.Id, Anita);

        // 7. The deviation closed by itself, and the document is on version 2.
        var closed = await db.Context.Deviations.AsNoTracking().FirstAsync(item => item.Id == deviation.Id);
        Assert.Equal(DeviationStatus.Closed, closed.Status);
        Assert.Equal(Anita.ToString(), closed.ClosedBy);
        Assert.Equal(2, (await db.Context.Documents.AsNoTracking().FirstAsync(item => item.Id == documentId)).CurrentVersion);

        // 8. The timeline shows every step, in order, with names.
        var timeline = await db.Queries.GetTimelineAsync(deviation.Id);
        Assert.Equal(
            [
                "Deviation raised",
                "Deviation accepted",
                "Report submitted (attempt 1)",
                $"Report {firstReport.Id} rejected",
                "Report submitted (attempt 2)",
                $"Report {secondReport.Id} accepted",
                $"Change request {changeRequest.Id} raised",
                $"Change request {changeRequest.Id} accepted",
                "Revision 2 uploaded",
                "Revision 2 approved",
                "Deviation closed"
            ],
            timeline!.Select(entry => entry.Step));
        Assert.Equal("Ravi Kumar", timeline[0].Who);
        Assert.Equal("Anita Das", timeline[^1].Who);

        // 9. The audit log holds the whole chain under the deviation's id.
        var auditActions = await db.Context.AuditLogs.Where(log => log.DeviationId == deviation.Id).Select(log => log.Action).ToListAsync();
        Assert.Contains("Change request submitted", auditActions);
        Assert.Contains("Revision 2 approved", auditActions);
        Assert.Contains("Deviation closed", auditActions);
    }

    [Fact]
    public async Task Report_accepted_with_no_change_needed_closes_the_deviation_at_once()
    {
        var deviation = await AcceptedDeviationAsync();
        var report = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);

        await db.Modules.ReviewReportAsync(report.Id, Suresh, accept: true, changeRequired: false);

        Assert.Equal(DeviationStatus.Closed, await StatusOf(deviation.Id));
        Assert.Empty(await db.Queries.GetReportsReadyForChangeRequestAsync());
    }

    [Fact]
    public async Task Rejected_change_request_sends_the_deviation_back_and_allows_a_new_request()
    {
        var deviation = await AcceptedDeviationAsync();
        var report = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);
        await db.Modules.ReviewReportAsync(report.Id, Suresh, accept: true, changeRequired: true);
        var firstRequest = await db.Modules.RaiseChangeRequestAsync(report.Id, Ravi);
        await db.Modules.SubmitChangeRequestAsync(firstRequest.Id, Ravi);

        await db.Modules.DecideChangeRequestAsync(firstRequest.Id, Suresh, approve: false);

        Assert.Equal(DeviationStatus.Investigating, await StatusOf(deviation.Id));
        Assert.Contains(await db.Queries.GetReportsReadyForChangeRequestAsync(), item => item.ReportId == report.Id);
        var secondRequest = await db.Modules.RaiseChangeRequestAsync(report.Id, Ravi);
        Assert.NotEqual(firstRequest.Id, secondRequest.Id);
    }

    // ------------------------------------------------------------- rules

    [Fact]
    public async Task A_deviation_needs_an_approved_document()
    {
        var draft = await db.Modules.CreateDocumentAsync("SOP-020", Ravi);

        var error = await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.RaiseDeviationAsync(draft.DocumentId, Ravi));
        Assert.Contains("not approved", error.Message);
    }

    [Fact]
    public async Task Only_one_report_can_wait_for_review()
    {
        var deviation = await AcceptedDeviationAsync();
        await db.Modules.SubmitReportAsync(deviation.Id, Ravi);

        await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.SubmitReportAsync(deviation.Id, Ravi));
    }

    [Fact]
    public async Task A_report_needs_an_accepted_deviation()
    {
        var document = await ApprovedDocumentAsync();
        var deviation = await db.Modules.RaiseDeviationAsync(document, Ravi);

        await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.SubmitReportAsync(deviation.Id, Ravi));
    }

    [Fact]
    public async Task A_change_request_needs_a_report_accepted_with_change_required()
    {
        var deviation = await AcceptedDeviationAsync();
        var report = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);

        await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.RaiseChangeRequestAsync(report.Id, Ravi));
    }

    [Fact]
    public async Task Only_one_change_request_can_be_active_per_report()
    {
        var report = await ReportReadyForChangeAsync();
        await db.Modules.RaiseChangeRequestAsync(report, Ravi);

        await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.RaiseChangeRequestAsync(report, Ravi));
    }

    [Fact]
    public async Task A_revision_of_an_approved_document_needs_an_approved_change_request()
    {
        var document = await ApprovedDocumentAsync();

        var error = await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.UploadRevisionAsync(document, null, Ravi));
        Assert.Contains("needs an approved change request", error.Message);
    }

    [Fact]
    public async Task A_change_request_cannot_revise_a_different_document()
    {
        var report = await ReportReadyForChangeAsync();
        var changeRequest = await db.Modules.RaiseChangeRequestAsync(report, Ravi);
        await db.Modules.SubmitChangeRequestAsync(changeRequest.Id, Ravi);
        await db.Modules.DecideChangeRequestAsync(changeRequest.Id, Suresh, approve: true);
        var otherDocument = await ApprovedDocumentAsync("SOP-099");

        var error = await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.UploadRevisionAsync(otherDocument, changeRequest.Id, Ravi));
        Assert.Contains("different document", error.Message);
    }

    [Fact]
    public async Task A_supervisor_cannot_review_their_own_item()
    {
        var document = await ApprovedDocumentAsync();
        var deviation = await db.Modules.RaiseDeviationAsync(document, Suresh);

        await Assert.ThrowsAsync<WorkflowException>(() => db.Modules.ReviewDeviationAsync(deviation.Id, Suresh, accept: true));
    }

    [Fact]
    public async Task Manual_close_needs_an_accepted_report()
    {
        var deviation = await AcceptedDeviationAsync();

        var error = await Assert.ThrowsAsync<WorkflowException>(() => db.Workflow.CloseDeviationAsync(deviation.Id, Suresh, "Done"));
        Assert.Contains("no accepted investigation report", error.Message);
    }

    [Fact]
    public async Task Manual_close_is_blocked_while_a_change_is_in_progress()
    {
        var report = await ReportReadyForChangeAsync();
        var changeRequest = await db.Modules.RaiseChangeRequestAsync(report, Ravi);
        await db.Modules.SubmitChangeRequestAsync(changeRequest.Id, Ravi);

        var error = await Assert.ThrowsAsync<WorkflowException>(() => db.Workflow.CloseDeviationAsync(changeRequest.DeviationId, Suresh, "Done"));
        Assert.Contains("still in progress", error.Message);
    }

    [Fact]
    public async Task Manual_close_works_after_a_rejected_change_request()
    {
        var report = await ReportReadyForChangeAsync();
        var changeRequest = await db.Modules.RaiseChangeRequestAsync(report, Ravi);
        await db.Modules.SubmitChangeRequestAsync(changeRequest.Id, Ravi);
        await db.Modules.DecideChangeRequestAsync(changeRequest.Id, Suresh, approve: false);

        await db.Workflow.CloseDeviationAsync(changeRequest.DeviationId, Suresh, "Risk accepted, no change");

        Assert.Equal(DeviationStatus.Closed, await StatusOf(changeRequest.DeviationId));
    }

    // ----------------------------------------------------------- helpers

    private async Task<int> ApprovedDocumentAsync(string number = "SOP-001")
    {
        var firstVersion = await db.Modules.CreateDocumentAsync(number, Ravi);
        await db.Modules.ApproveRevisionAsync(firstVersion.Id, Suresh);
        return firstVersion.DocumentId;
    }

    private async Task<QMSSystem.Shared.Models.Deviation> AcceptedDeviationAsync()
    {
        var deviation = await db.Modules.RaiseDeviationAsync(await ApprovedDocumentAsync(), Ravi);
        await db.Modules.ReviewDeviationAsync(deviation.Id, Suresh, accept: true);
        return deviation;
    }

    private async Task<int> ReportReadyForChangeAsync()
    {
        var deviation = await AcceptedDeviationAsync();
        var report = await db.Modules.SubmitReportAsync(deviation.Id, Ravi);
        await db.Modules.ReviewReportAsync(report.Id, Suresh, accept: true, changeRequired: true);
        return report.Id;
    }

    private async Task<DeviationStatus> StatusOf(int deviationId) =>
        (await db.Context.Deviations.AsNoTracking().FirstAsync(item => item.Id == deviationId)).Status;
}
