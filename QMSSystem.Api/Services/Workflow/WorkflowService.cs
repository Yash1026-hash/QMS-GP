using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Enums;
using QMSSystem.Shared.Models;
using QMSSystem.Shared.Workflow;

namespace QMSSystem.Api.Services.Workflow;

// Connects the three modules. Module services call it in two ways:
//
// 1. Ensure... methods (guards): call BEFORE you create or change your record.
//    They throw WorkflowException when the action breaks a rule.
// 2. On... methods (handoffs): call AFTER you set your record's new status.
//    They update the other modules and save everything in one SaveChanges,
//    so you do not need to call SaveChanges yourself.
//
// See docs/INTEGRATION.md for the one line each module adds.
public sealed class WorkflowService(QmsDbContext context, AuditService audit, TimeProvider clock)
{
    // ---------------------------------------------------------------- guards

    // Deviations module, before creating a deviation.
    public async Task EnsureCanRaiseDeviationAsync(int documentId)
    {
        var document = await context.Documents.FindAsync(documentId)
            ?? throw WorkflowException.NotFound(ItemTypes.Document, documentId);

        if (document.Status != DocumentStatuses.Approved)
        {
            throw new WorkflowException(
                $"Document {document.DocumentNumber} is not approved. " +
                "A deviation can only be raised against an approved document.");
        }
    }

    // Deviations module, before saving a new report. Returns the attempt number to use.
    public async Task<int> EnsureCanSubmitReportAsync(int deviationId)
    {
        var deviation = await LoadDeviationWithReportsAsync(deviationId);

        if (deviation.Status != DeviationStatus.Investigating)
        {
            throw new WorkflowException(
                $"Only an accepted deviation can get a report. This deviation is {deviation.Status}.");
        }

        if (deviation.Reports.Any(report => report.Status == ReportStatus.Pending))
        {
            throw new WorkflowException("This deviation already has a report waiting for review.");
        }

        if (deviation.Reports.Any(report => report.Status == ReportStatus.Accepted))
        {
            throw new WorkflowException("This deviation already has an accepted report.");
        }

        return deviation.Reports.Count == 0
            ? 1
            : deviation.Reports.Max(report => report.AttemptNumber) + 1;
    }

    // Change request module, before creating a change request.
    // Returns the deviation and document the new change request must point to.
    public async Task<ChangeRequestLink> EnsureCanRaiseChangeRequestAsync(int deviationReportId)
    {
        var report = await context.DeviationReports
            .Include(item => item.Deviation)
            .FirstOrDefaultAsync(item => item.Id == deviationReportId)
            ?? throw WorkflowException.NotFound(ItemTypes.Report, deviationReportId);

        if (report.Status != ReportStatus.Accepted || report.ChangeRequired != true)
        {
            throw new WorkflowException(
                "A change request needs a deviation report that was accepted with 'change required'.");
        }

        if (report.Deviation!.Status != DeviationStatus.Investigating)
        {
            throw new WorkflowException(
                $"The deviation is {report.Deviation.Status}, so a new change request cannot be raised.");
        }

        var hasActiveRequest = await context.ChangeRequests.AnyAsync(changeRequest =>
            changeRequest.DeviationReportId == deviationReportId &&
            changeRequest.Status != ChangeRequestStatuses.Rejected);
        if (hasActiveRequest)
        {
            throw new WorkflowException(
                "This report already has an active change request. Only one can be active at a time.");
        }

        return new ChangeRequestLink(report.DeviationId, report.Deviation.DocumentId, report.Id);
    }

    // Documents module, before saving a new revision.
    // Version 1 of a draft document needs no change request.
    // Every revision of an approved document needs an approved change request.
    public async Task EnsureCanUploadRevisionAsync(int documentId, int? changeRequestId)
    {
        var document = await context.Documents.FindAsync(documentId)
            ?? throw WorkflowException.NotFound(ItemTypes.Document, documentId);

        if (document.Status == DocumentStatuses.Obsolete)
        {
            throw new WorkflowException($"Document {document.DocumentNumber} is obsolete.");
        }

        var hasPendingRevision = await context.DocumentRevisions.AnyAsync(revision =>
            revision.DocumentId == documentId &&
            revision.ApprovalStatus == RevisionStatuses.Pending);
        if (hasPendingRevision)
        {
            throw new WorkflowException("This document already has a revision waiting for approval.");
        }

        if (document.Status != DocumentStatuses.Approved)
        {
            return;
        }

        if (changeRequestId is null)
        {
            throw new WorkflowException(
                "A new revision of an approved document needs an approved change request.");
        }

        var changeRequest = await context.ChangeRequests.FindAsync(changeRequestId.Value)
            ?? throw WorkflowException.NotFound(ItemTypes.ChangeRequest, changeRequestId.Value);

        if (changeRequest.Status != ChangeRequestStatuses.Approved)
        {
            throw new WorkflowException($"Change request {changeRequest.Id} is not approved.");
        }

        if (changeRequest.DocumentId != documentId)
        {
            throw new WorkflowException(
                $"Change request {changeRequest.Id} is for a different document.");
        }

        var alreadyImplemented = await context.DocumentRevisions.AnyAsync(revision =>
            revision.ChangeRequestId == changeRequest.Id &&
            revision.ApprovalStatus == RevisionStatuses.Approved);
        if (alreadyImplemented)
        {
            throw new WorkflowException(
                $"Change request {changeRequest.Id} already has an approved revision.");
        }
    }

    // Every review page, before saving a decision. createdBy is the item's
    // CreatedBy / UploadedBy / RequestedByUserId value.
    public static void EnsureNotOwnItem(string? createdBy, int reviewerUserId)
    {
        if (string.Equals(createdBy?.Trim(), reviewerUserId.ToString(), StringComparison.Ordinal))
        {
            throw new WorkflowException("You cannot review an item that you raised.");
        }
    }

    public static void EnsureNotOwnItem(int createdByUserId, int reviewerUserId) =>
        EnsureNotOwnItem(createdByUserId.ToString(), reviewerUserId);

    // -------------------------------------------------------------- handoffs

    // Deviations module, after setting report.Status = Accepted.
    // changeRequired = false closes the deviation now.
    // changeRequired = true lets the operator raise a change request from this report.
    public async Task OnReportAcceptedAsync(int deviationReportId, bool changeRequired, int actorUserId)
    {
        var report = await context.DeviationReports
            .Include(item => item.Deviation)
            .FirstOrDefaultAsync(item => item.Id == deviationReportId)
            ?? throw WorkflowException.NotFound(ItemTypes.Report, deviationReportId);

        if (report.Status != ReportStatus.Accepted)
        {
            throw new WorkflowException("Set the report status to Accepted before calling OnReportAcceptedAsync.");
        }

        report.ChangeRequired = changeRequired;
        audit.Record(
            ItemTypes.Report,
            report.Id,
            changeRequired ? "Report accepted: change required" : "Report accepted: no change needed",
            actorUserId,
            report.DeviationId);

        if (!changeRequired)
        {
            Close(report.Deviation!, actorUserId, "No change needed");
        }

        await context.SaveChangesAsync();
    }

    // Change request module, after setting Status = Submitted.
    public async Task OnChangeRequestSubmittedAsync(int changeRequestId, int actorUserId)
    {
        var changeRequest = await LoadChangeRequestAsync(changeRequestId, ChangeRequestStatuses.Submitted);
        var deviation = await LoadDeviationAsync(changeRequest.DeviationId);

        if (deviation.Status == DeviationStatus.Investigating)
        {
            deviation.Status = DeviationStatus.AwaitingChange;
        }

        audit.Record(ItemTypes.ChangeRequest, changeRequest.Id, "Change request submitted", actorUserId, deviation.Id);
        await context.SaveChangesAsync();
    }

    // Change request module, after setting Status = Approved.
    // The change request now appears in the Upload Revision picker for its document.
    public async Task OnChangeRequestApprovedAsync(int changeRequestId, int actorUserId)
    {
        var changeRequest = await LoadChangeRequestAsync(changeRequestId, ChangeRequestStatuses.Approved);

        audit.Record(ItemTypes.ChangeRequest, changeRequest.Id, "Change request approved", actorUserId, changeRequest.DeviationId);
        await context.SaveChangesAsync();
    }

    // Change request module, after setting Status = Rejected.
    // A rejected request cannot be resubmitted, so the deviation goes back to
    // Investigating and the operator can raise a new change request.
    public async Task OnChangeRequestRejectedAsync(int changeRequestId, int actorUserId)
    {
        var changeRequest = await LoadChangeRequestAsync(changeRequestId, ChangeRequestStatuses.Rejected);
        var deviation = await LoadDeviationAsync(changeRequest.DeviationId);

        var otherRequestInProgress = await context.ChangeRequests.AnyAsync(other =>
            other.DeviationId == deviation.Id &&
            other.Id != changeRequest.Id &&
            (other.Status == ChangeRequestStatuses.Submitted || other.Status == ChangeRequestStatuses.Approved));

        if (deviation.Status == DeviationStatus.AwaitingChange && !otherRequestInProgress)
        {
            deviation.Status = DeviationStatus.Investigating;
        }

        audit.Record(ItemTypes.ChangeRequest, changeRequest.Id, "Change request rejected", actorUserId, deviation.Id);
        await context.SaveChangesAsync();
    }

    // Documents module, after setting ApprovalStatus = Approved.
    // When the revision implements a change request, the deviation behind it closes.
    public async Task OnRevisionApprovedAsync(int revisionId, int actorUserId)
    {
        var revision = await context.DocumentRevisions.FindAsync(revisionId)
            ?? throw WorkflowException.NotFound(ItemTypes.Revision, revisionId);

        if (revision.ApprovalStatus != RevisionStatuses.Approved)
        {
            throw new WorkflowException("Set the revision status to Approved before calling OnRevisionApprovedAsync.");
        }

        int? deviationId = null;
        if (revision.ChangeRequestId is int changeRequestId)
        {
            var changeRequest = await context.ChangeRequests.FindAsync(changeRequestId)
                ?? throw WorkflowException.NotFound(ItemTypes.ChangeRequest, changeRequestId);
            var deviation = await LoadDeviationAsync(changeRequest.DeviationId);
            deviationId = deviation.Id;

            if (deviation.Status == DeviationStatus.AwaitingChange)
            {
                Close(deviation, actorUserId, $"Closed automatically: revision {revision.Version} approved");
            }
        }

        audit.Record(ItemTypes.Revision, revision.Id, $"Revision {revision.Version} approved", actorUserId, deviationId);
        await context.SaveChangesAsync();
    }

    // Supervisor closes a deviation by hand, for example after a rejected change
    // request when no new change is needed.
    public async Task CloseDeviationAsync(int deviationId, int actorUserId, string reason)
    {
        var deviation = await LoadDeviationWithReportsAsync(deviationId);

        if (deviation.Status is DeviationStatus.Closed or DeviationStatus.Rejected)
        {
            throw new WorkflowException($"The deviation is already {deviation.Status}.");
        }

        if (!deviation.Reports.Any(report => report.Status == ReportStatus.Accepted))
        {
            throw new WorkflowException("Cannot close: the deviation has no accepted investigation report.");
        }

        var changeInProgress = await context.ChangeRequests.AnyAsync(changeRequest =>
            changeRequest.DeviationId == deviationId &&
            (changeRequest.Status == ChangeRequestStatuses.Submitted ||
             (changeRequest.Status == ChangeRequestStatuses.Approved &&
              !context.DocumentRevisions.Any(revision =>
                  revision.ChangeRequestId == changeRequest.Id &&
                  revision.ApprovalStatus == RevisionStatuses.Approved))));
        if (changeInProgress)
        {
            throw new WorkflowException("Cannot close: a change request for this deviation is still in progress.");
        }

        Close(deviation, actorUserId, reason);
        await context.SaveChangesAsync();
    }

    // --------------------------------------------------------------- helpers

    private void Close(Deviation deviation, int actorUserId, string reason)
    {
        deviation.Status = DeviationStatus.Closed;
        deviation.ClosedBy = actorUserId.ToString();
        deviation.ClosedDate = clock.GetUtcNow().UtcDateTime;
        audit.Record(ItemTypes.Deviation, deviation.Id, "Deviation closed", actorUserId, deviation.Id, reason);
    }

    private async Task<Deviation> LoadDeviationAsync(int deviationId) =>
        await context.Deviations.FindAsync(deviationId)
            ?? throw WorkflowException.NotFound(ItemTypes.Deviation, deviationId);

    private async Task<Deviation> LoadDeviationWithReportsAsync(int deviationId) =>
        await context.Deviations
            .Include(deviation => deviation.Reports)
            .FirstOrDefaultAsync(deviation => deviation.Id == deviationId)
            ?? throw WorkflowException.NotFound(ItemTypes.Deviation, deviationId);

    private async Task<ChangeRequest> LoadChangeRequestAsync(int changeRequestId, string expectedStatus)
    {
        var changeRequest = await context.ChangeRequests.FindAsync(changeRequestId)
            ?? throw WorkflowException.NotFound(ItemTypes.ChangeRequest, changeRequestId);

        if (changeRequest.Status != expectedStatus)
        {
            throw new WorkflowException(
                $"Set the change request status to {expectedStatus} before calling the workflow.");
        }

        return changeRequest;
    }
}

// What a new change request must point to.
public sealed record ChangeRequestLink(int DeviationId, int DocumentId, int DeviationReportId);
