using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Api.Services.Workflow;
using QMSSystem.Shared.Enums;
using QMSSystem.Shared.Models;
using QMSSystem.Shared.Workflow;

namespace QMSSystem.Tests;

// What each module's service does, written the way the module owners should
// write it: guard first, change your own record, then call the handoff.
// Module owners can copy these methods as a starting point.
public sealed class ModuleActions(QmsDbContext context, WorkflowService workflow, TestClock clock)
{
    // ------------------------------------------------------------ Documents

    public async Task<DocumentRevision> CreateDocumentAsync(string number, int operatorId)
    {
        Tick();
        var document = new Document
        {
            DocumentNumber = number,
            Title = $"{number} procedure",
            Department = "Quality",
            CurrentVersion = 0,
            Status = DocumentStatuses.Draft,
            CreatedBy = operatorId,
            CreationTime = clock.Now
        };
        var firstVersion = new DocumentRevision
        {
            Document = document,
            Version = 1,
            FileName = $"{number}-v1.pdf",
            UploadedBy = operatorId.ToString(),
            UploadedTime = clock.Now,
            ApprovalStatus = RevisionStatuses.Pending
        };
        context.DocumentRevisions.Add(firstVersion);
        await context.SaveChangesAsync();
        return firstVersion;
    }

    public async Task<DocumentRevision> UploadRevisionAsync(int documentId, int? changeRequestId, int operatorId)
    {
        Tick();
        await workflow.EnsureCanUploadRevisionAsync(documentId, changeRequestId);

        var document = await context.Documents.FindAsync(documentId);
        var revision = new DocumentRevision
        {
            DocumentId = documentId,
            Version = document!.CurrentVersion + 1,
            FileName = $"{document.DocumentNumber}-v{document.CurrentVersion + 1}.pdf",
            UploadedBy = operatorId.ToString(),
            UploadedTime = clock.Now,
            ChangeSummary = "Updated after change request",
            ApprovalStatus = RevisionStatuses.Pending,
            ChangeRequestId = changeRequestId
        };
        context.DocumentRevisions.Add(revision);
        await context.SaveChangesAsync();
        return revision;
    }

    public async Task ApproveRevisionAsync(int revisionId, int supervisorId)
    {
        Tick();
        var revision = await context.DocumentRevisions.Include(item => item.Document).FirstAsync(item => item.Id == revisionId);
        WorkflowService.EnsureNotOwnItem(revision.UploadedBy, supervisorId);

        revision.ApprovalStatus = RevisionStatuses.Approved;
        revision.ApprovedBy = supervisorId.ToString();
        revision.ApprovalDate = clock.Now;
        revision.Document.CurrentVersion = revision.Version;
        revision.Document.Status = DocumentStatuses.Approved;

        await workflow.OnRevisionApprovedAsync(revision.Id, supervisorId); // saves everything
    }

    // ----------------------------------------------------------- Deviations

    public async Task<Deviation> RaiseDeviationAsync(int documentId, int operatorId, string title = "Test done outside specification")
    {
        Tick();
        await workflow.EnsureCanRaiseDeviationAsync(documentId);

        var deviation = new Deviation
        {
            DocumentId = documentId,
            Title = title,
            Description = title,
            Priority = Priority.Major,
            CreatedBy = operatorId.ToString(),
            CreatedDate = clock.Now
        };
        context.Deviations.Add(deviation);
        await context.SaveChangesAsync();
        return deviation;
    }

    public async Task ReviewDeviationAsync(int deviationId, int supervisorId, bool accept)
    {
        Tick();
        var deviation = await context.Deviations.FindAsync(deviationId);
        WorkflowService.EnsureNotOwnItem(deviation!.CreatedBy, supervisorId);

        deviation.Status = accept ? DeviationStatus.Investigating : DeviationStatus.Rejected;
        AddDecision(ItemTypes.Deviation, deviationId, accept, supervisorId);
        await context.SaveChangesAsync();
    }

    public async Task<DeviationReport> SubmitReportAsync(int deviationId, int operatorId)
    {
        Tick();
        var attempt = await workflow.EnsureCanSubmitReportAsync(deviationId);

        var report = new DeviationReport
        {
            DeviationId = deviationId,
            AttemptNumber = attempt,
            RootCause = $"Root cause, attempt {attempt}",
            CorrectiveAction = "Update the SOP",
            Summary = "Investigation summary",
            FileName = $"report-{deviationId}-{attempt}.pdf",
            CreatedBy = operatorId.ToString(),
            CreatedDate = clock.Now
        };
        context.DeviationReports.Add(report);
        await context.SaveChangesAsync();
        return report;
    }

    public async Task ReviewReportAsync(int reportId, int supervisorId, bool accept, bool changeRequired = true)
    {
        Tick();
        var report = await context.DeviationReports.FindAsync(reportId);
        WorkflowService.EnsureNotOwnItem(report!.CreatedBy, supervisorId);

        report.Status = accept ? ReportStatus.Accepted : ReportStatus.Rejected;
        AddDecision(ItemTypes.Report, reportId, accept, supervisorId);

        if (accept)
        {
            await workflow.OnReportAcceptedAsync(reportId, changeRequired, supervisorId); // saves everything
        }
        else
        {
            await context.SaveChangesAsync();
        }
    }

    // ------------------------------------------------------ Change requests

    public async Task<ChangeRequest> RaiseChangeRequestAsync(int reportId, int operatorId)
    {
        Tick();
        var link = await workflow.EnsureCanRaiseChangeRequestAsync(reportId);

        var changeRequest = new ChangeRequest
        {
            DeviationReportId = link.DeviationReportId,
            DeviationId = link.DeviationId,
            DocumentId = link.DocumentId,
            Title = "Update the procedure",
            Description = "Change the SOP step",
            ChangeType = ChangeTypes.Process,
            Status = ChangeRequestStatuses.Draft,
            RequestedByUserId = operatorId,
            RequestedDate = clock.Now
        };
        context.ChangeRequests.Add(changeRequest);
        await context.SaveChangesAsync();
        return changeRequest;
    }

    public async Task SubmitChangeRequestAsync(int changeRequestId, int operatorId)
    {
        Tick();
        var changeRequest = await context.ChangeRequests.FindAsync(changeRequestId);
        changeRequest!.Status = ChangeRequestStatuses.Submitted;

        await workflow.OnChangeRequestSubmittedAsync(changeRequestId, operatorId); // saves everything
    }

    public async Task DecideChangeRequestAsync(int changeRequestId, int supervisorId, bool approve)
    {
        Tick();
        var changeRequest = await context.ChangeRequests.FindAsync(changeRequestId);
        WorkflowService.EnsureNotOwnItem(changeRequest!.RequestedByUserId, supervisorId);

        changeRequest.Status = approve ? ChangeRequestStatuses.Approved : ChangeRequestStatuses.Rejected;
        AddDecision(ItemTypes.ChangeRequest, changeRequestId, approve, supervisorId);

        if (approve)
        {
            await workflow.OnChangeRequestApprovedAsync(changeRequestId, supervisorId); // saves everything
        }
        else
        {
            await workflow.OnChangeRequestRejectedAsync(changeRequestId, supervisorId); // saves everything
        }
    }

    // -------------------------------------------------------------- helpers

    private void AddDecision(string itemType, int itemId, bool accept, int supervisorId) =>
        context.ApprovalRecords.Add(new ApprovalRecord
        {
            ItemType = itemType,
            ItemId = itemId,
            Decision = accept ? Decisions.Accepted : Decisions.Rejected,
            Comments = accept ? "OK" : "Not OK",
            ReviewedByUserId = supervisorId,
            DecisionDate = clock.Now
        });

    private void Tick() => clock.Advance(TimeSpan.FromHours(1));
}
