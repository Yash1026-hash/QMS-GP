# QMS integration guide

Owner: Sukruth (integration). Read this before you write your module's service or page.

Each of us builds one part of the app. The integration layer connects the parts, so that the full journey works:

```
Approved SOP → Deviation → Deviation report → Change request → New SOP revision → Deviation closed
```

You only need to do three things: use the shared database context, use the shared status words, and add one line (two at most) to your service.

## 1. Rules for everyone

1. **Use the shared database mappings.** Workflow data uses `QmsDbContext` (`QMSSystem.Api/Data/QmsDbContext.cs`); both it and the document upload API map documents to `dbo.Documents`. Do not introduce a separate document table.
2. **Use the status constants, never typed words.** Write `DocumentStatuses.Approved`, not `"Approved"` or `"approved"`. They are in `QMSSystem.Shared/Workflow/WorkflowValues.cs`.
3. **"Who" fields hold the user's `UserId`** from login (`ClaimTypes.NameIdentifier`). The API has no login of its own, so the Web page sends the user id in the request body.
4. **Business rules go in the API service, not in the page.** Hiding a button is not enough.
5. **Do not rename or remove shared model fields.** If you need a change, tell the integration owner first.
6. **Pages go in the role folder.** Operator pages in `Pages/Operator/...`, supervisor pages in `Pages/Supervisor/...`. Only those folders get the role check (`QMSSystem.Web/Program.cs`).
7. **Document uploads use `dbo.Documents`.** The create-document API stores the file name, content type, bytes, and comment on the document row through `ApplicationDbContext`. Ensure the upload columns exist by running `database/document-file-columns.sql` against the configured database. This upload path does not create a `DocumentRevision`.

## 2. Status words

| Record | Field | Values | Where |
| --- | --- | --- | --- |
| Document | `Status` (text) | Draft, Approved, Obsolete | `DocumentStatuses` |
| DocumentRevision | `ApprovalStatus` (text) | Pending, Approved, Rejected | `RevisionStatuses` |
| Deviation | `Status` (enum) | Open, Investigating, AwaitingChange, Closed, Rejected | `DeviationStatus` |
| DeviationReport | `Status` (enum) | Pending, Accepted, Rejected | `ReportStatus` |
| ChangeRequest | `Status` (text) | Draft, Submitted, Approved, Rejected | `ChangeRequestStatuses` |
| ChangeRequest | `ChangeType` (text) | Process, Equipment, Software | `ChangeTypes` |
| ApprovalRecord | `Decision` (text) | Accepted, Rejected | `Decisions` |
| ApprovalRecord, AuditLog | `ItemType` (text) | Document, Revision, Deviation, Report, ChangeRequest | `ItemTypes` |

`Deviation.Priority` keeps its name and holds the severity: Minor, Major, Critical.

## 3. How the tables link

```
Document 1 ──< DocumentRevision            (DocumentRevision.DocumentId)
Document 1 ──< Deviation                   (Deviation.DocumentId)
Deviation 1 ──< DeviationReport            (DeviationReport.DeviationId, AttemptNumber 1, 2, 3 …)
DeviationReport 1 ──< ChangeRequest        (ChangeRequest.DeviationReportId; also DeviationId, DocumentId)
ChangeRequest 1 ──< DocumentRevision       (DocumentRevision.ChangeRequestId; empty for version 1)
ApprovalRecord: one supervisor decision per item (ItemType + ItemId)
AuditLog: every action, with DeviationId when it belongs to a deviation
```

Every supervisor **decision** (accept, reject, approve) is an `ApprovalRecord`. Do not make a separate approval table for your module: `ChangeRequestApproval` and `ReviewApproval` are not mapped and must not be used.

New fields added for integration: `Deviation.ClosedBy`, `Deviation.ClosedDate`, `DeviationReport.RootCause`, `DeviationReport.CorrectiveAction`, `DeviationReport.ChangeRequired`, `ChangeRequest.Status`, `ChangeRequest.DeviationReportId`, `DocumentRevision.ChangeRequestId`, and the status value `DeviationStatus.AwaitingChange`.

## 4. The line you add to your service

Inject `WorkflowService workflow` into your API service. Then:

- a **guard** (`Ensure…`) goes **before** you create or change your record. It throws `WorkflowException` when the action breaks a rule. The API turns that into HTTP 409 with the message, so your page can show it.
- a **handoff** (`On…`) goes **after** you set your record's new status. It updates the other modules and calls `SaveChanges` for everything, so you do not call `SaveChanges` again.

| Your action | Page | Before (guard) | After (handoff) |
| --- | --- | --- | --- |
| Upload a revision | `/Documents/UploadRevision` | `await workflow.EnsureCanUploadRevisionAsync(documentId, changeRequestId);` | — |
| Approve a revision | `/Documents/Review` | `WorkflowService.EnsureNotOwnItem(revision.UploadedBy, supervisorId);` | `await workflow.OnRevisionApprovedAsync(revision.Id, supervisorId);` |
| Raise a deviation | `/Deviations/Create` | `await workflow.EnsureCanRaiseDeviationAsync(documentId);` | — |
| Accept or reject a deviation | `/Deviations/Review` | `WorkflowService.EnsureNotOwnItem(deviation.CreatedBy, supervisorId);` | — |
| Submit a report | `/Deviations/SubmitReport` | `var attempt = await workflow.EnsureCanSubmitReportAsync(deviationId);` | — |
| Accept a report | `/Deviations/ReviewReport` | `WorkflowService.EnsureNotOwnItem(report.CreatedBy, supervisorId);` | `await workflow.OnReportAcceptedAsync(report.Id, changeRequired, supervisorId);` |
| Raise a change request | `/ChangeRequests/Edit` | `var link = await workflow.EnsureCanRaiseChangeRequestAsync(reportId);` then copy `link.DeviationId`, `link.DocumentId`, `link.DeviationReportId` | — |
| Submit a change request | `/ChangeRequests/Edit` | — | `await workflow.OnChangeRequestSubmittedAsync(changeRequest.Id, operatorId);` |
| Approve a change request | `/ChangeRequests/Approve` | `WorkflowService.EnsureNotOwnItem(changeRequest.RequestedByUserId, supervisorId);` | `await workflow.OnChangeRequestApprovedAsync(changeRequest.Id, supervisorId);` |
| Reject a change request | `/ChangeRequests/Approve` | same as approve | `await workflow.OnChangeRequestRejectedAsync(changeRequest.Id, supervisorId);` |

**Working examples** of every row are in `QMSSystem.Tests/ModuleActions.cs`. Copy the method for your action as a starting point.

Example: approving a change request.

```csharp
var changeRequest = await context.ChangeRequests.FindAsync(id);
WorkflowService.EnsureNotOwnItem(changeRequest.RequestedByUserId, supervisorId);   // guard

changeRequest.Status = ChangeRequestStatuses.Approved;                             // your change
context.ApprovalRecords.Add(new ApprovalRecord { ItemType = ItemTypes.ChangeRequest, ItemId = id,
    Decision = Decisions.Accepted, Comments = comments, ReviewedByUserId = supervisorId });

await workflow.OnChangeRequestApprovedAsync(id, supervisorId);                     // handoff, saves all
```

## 5. Endpoints for your page's dropdowns and the timeline

| Page | Call | Returns |
| --- | --- | --- |
| Operator home `/Index` | `GET /api/workflow/documents/active` | Approved documents (show "Raise Deviation" on each) |
| `/Deviations/SubmitReport` | `GET /api/workflow/deviations/ready-for-report?reportedByUserId={id}` | Accepted deviations that need a report, with the next attempt number |
| `/ChangeRequests/Edit` | `GET /api/workflow/reports/ready-for-change-request` | Accepted reports that need a change and have no active request |
| `/Documents/UploadRevision` | `GET /api/workflow/documents/{documentId}/change-requests-ready-for-revision` | Approved change requests for that document |
| `/Deviations/Details` | `GET /api/workflow/deviations/{id}/timeline` | Every step with date, name and detail, oldest first |
| `/Deviations/Details` (Supervisor) | `POST /api/workflow/deviations/{id}/close` with `{ "actorUserId": 3, "reason": "…" }` | Closes a deviation by hand; refused while a change is in progress |

Try them in Swagger at `http://localhost:5070/swagger`.

## 6. Database and demo data

1. Run `database/login-schema.sql`, then `database/qms-schema.sql`. Both only create missing tables and never change existing ones.
2. Demo data, **for a local database only**: it adds one record at every workflow step (2 approved SOPs, 1 draft SOP, deviations in every status, a rejected and a pending report, change requests submitted and approved, one fully closed chain). Set these user secrets on the API project, with ids of real login accounts, then start the API:

```powershell
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Qms:SeedDemoData" "true"
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Qms:DemoUsers:Operator" "<user id>"
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Qms:DemoUsers:SecondOperator" "<user id>"
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Qms:DemoUsers:Supervisor" "<user id>"
dotnet user-secrets set --project .\QMSSystem.Api\QMSSystem.Api.csproj "Qms:DemoUsers:SecondSupervisor" "<user id>"
```

It does nothing if documents already exist. Never turn it on against the shared `TRG_CORE` database.

## 7. Tests

```powershell
dotnet test .\QMSSystem.Tests\QMSSystem.Tests.csproj
```

The tests run the full journey and every rule on an in-memory database, so they need no SQL Server. Run them before you open a pull request that touches the shared models or your service's workflow calls.

## 8. Open points for the team

- QMS workflow tables use the `KS_` prefix (`KS_Deviations`, `KS_ChangeRequests`, …); the shared document table is `dbo.Documents`. If you already created a QMS table in `TRG_CORE` under another name or shape, tell the integration owner before running `qms-schema.sql`.
- Who fields have mixed types today (`Document.CreatedBy` and `ChangeRequest.RequestedByUserId` are numbers; `Deviation.CreatedBy` and `DocumentRevision.UploadedBy` are text). All of them hold the `UserId`. A later clean-up can make them all numbers.
- The API has no authentication, so role checks happen in the Web pages (`/Supervisor`, `/Operator`, `/Admin` folder policies). Every page that calls a supervisor action must be in the `/Supervisor` folder.
