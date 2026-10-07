# Team integration status and messages

Owner: Sukruth (integration). Status on 7 Oct 2026.

## Where we are

- `master` has the integration layer (shared statuses, `QmsDbContext`, `WorkflowService`, `/api/workflow` endpoints, 19 tests).
- Every module has its **models**. No module has its **API service or page logic** yet, except Dawood's Documents pages (branch, not merged).
- So the next step for everyone is: write your service and page, using the line(s) for your action from `docs/INTEGRATION.md` section 4.

## Changes made by integration today

- `ReviewComment` and `ReviewProof` (Manikanta): namespace fixed to `QMSSystem.Shared.Models`, `ReviewReportId` renamed to `DeviationReportId` (there is no ReviewReport table; it links to `KS_DeviationReports`). Both are now saved by `QmsDbContext` (tables `KS_ReviewComments`, `KS_ReviewProofs` in `database/qms-schema.sql`).
- `ChangeRequestApproval` (Charan) and `ReviewApproval` (Manikanta) are **not used**. Supervisor decisions go in `ApprovalRecord`.
- `.github/CODEOWNERS`: PRs touching shared models, `QmsDbContext`, workflow services, `database/` or tests need the integration owner's review.
- `docs/INTEGRATION.md`: added rule 6 (role folders) and rule 7 (files belong to a revision).

## Merge order

Documents → Deviations → Reports → Change Requests → Revision approval. Each PR: `dotnet build` passes and `dotnet test QMSSystem.Tests` passes.

---

## Message to the whole team (post in the group)

> Hi all, Sukruth here (integration).
>
> The integration layer is merged into master. Before you write your service or page, please:
>
> 1. `git pull origin master` into your branch.
> 2. Read `docs/INTEGRATION.md` (10 minutes). Section 4 has a table with the exact line you add for your action. `QMSSystem.Tests/ModuleActions.cs` has a working example of every action — copy yours.
> 3. Follow these rules:
>    - Use `QmsDbContext` only. Don't make your own DbContext.
>    - Use status constants (`DocumentStatuses.Approved`, `DeviationStatus.Investigating`, `ChangeRequestStatuses.Submitted` …). Never type `"Approved"` by hand.
>    - "Who" fields store the logged-in user's `UserId`.
>    - Supervisor decisions go in `ApprovalRecord`. Don't create your own approval table.
>    - Operator pages in `Pages/Operator/...`, supervisor pages in `Pages/Supervisor/...` (otherwise there is no role check).
>    - Don't rename or remove fields in `QMSSystem.Shared/Models`. Ask me first.
> 4. Before your PR: `dotnet build` and `dotnet test QMSSystem.Tests` must pass. I review PRs that touch shared models, the DbContext or the database script.
>
> Workflow errors come back from the API as HTTP 409 with a message — show that message on your page.

---

## Message to each developer

### Dawood — Documents Create / Details
> Your Create and Details pages are good to see. Three changes before your PR:
> 1. Move the file fields (`FileName`, `ContentType`, `FileData`, `Comment`) from `Document` to `DocumentRevision`. Creating a document also creates revision 1 with the file (see `CreateDocumentAsync` in `ModuleActions.cs`). Sai Teja's upload uses the same fields, so agree on them with him.
> 2. Move the pages to `Pages/Operator/Documents/...` (Create) so the role check applies.
> 3. Rename `Index.csthml.cs` → `Index.cshtml.cs` (spelling).
> New document status = `DocumentStatuses.Draft`; it becomes `Approved` only when revision 1 is approved.

### Sai Teja — Documents Upload Revision
> Before saving: `await workflow.EnsureCanUploadRevisionAsync(documentId, changeRequestId);`
> Fill the change request dropdown from `GET /api/workflow/documents/{documentId}/change-requests-ready-for-revision` and save the chosen id in `DocumentRevision.ChangeRequestId`. New revision `ApprovalStatus = RevisionStatuses.Pending`. Example: `UploadRevisionAsync` in `ModuleActions.cs`. Please agree the file fields with Dawood (they live on `DocumentRevision`). Remove the commented-out `Pages/Operator/UploadVersion` file if it is not used.

### Yuvaraj — Documents Review (Supervisor)
> Page goes in `Pages/Supervisor/Documents/...`. Before approving: `WorkflowService.EnsureNotOwnItem(revision.UploadedBy, supervisorId);`. Set `ApprovalStatus = RevisionStatuses.Approved`, `ApprovedBy`, `ApprovalDate`, and on the document `CurrentVersion = revision.Version` and `Status = DocumentStatuses.Approved`. Then `await workflow.OnRevisionApprovedAsync(revision.Id, supervisorId);` — this closes the linked deviation and saves everything. Example: `ApproveRevisionAsync`.

### Ishtiyaq — Operator home
> Show the list from `GET /api/workflow/documents/active` with a "Raise Deviation" button on each row that opens Nithya's Create page with the `documentId`.

### Satwik — Supervisor home
> Show counts / lists of items waiting for a supervisor: deviations with status `Open`, reports with `ReportStatus.Pending`, change requests with `ChangeRequestStatuses.Submitted`, revisions with `RevisionStatuses.Pending`. Tell me if you want one API endpoint for all four counts and I'll add it.

### Nithya — Deviations Create (Operator)
> Page in `Pages/Operator/Deviations/...`. Before saving: `await workflow.EnsureCanRaiseDeviationAsync(documentId);` (only approved SOPs). New deviation `Status = DeviationStatus.Open`, `CreatedBy` = user id. `Priority` holds Minor / Major / Critical. Example: `RaiseDeviationAsync`.

### Gayathri — Deviations Index
> Your branch has empty files only. Build the list with status from `DeviationStatus` (Open, Investigating, AwaitingChange, Closed, Rejected). Put the pages in `Pages/Operator/Deviations/...` (operator list) or `Pages/Supervisor/Deviations/...` (supervisor list). Agree with Nithya, Harshitha and Adhwika so you don't create the same files (`Create`, `Details`, `Report`).

### Harshitha — Deviations Details
> Show the traceability timeline from `GET /api/workflow/deviations/{id}/timeline`. For supervisors only, add a "Close deviation" button calling `POST /api/workflow/deviations/{id}/close` with `{ "actorUserId": <id>, "reason": "…" }`; show the 409 message if the API refuses.

### Adhwika — Deviations Report (Submit)
> Fill the deviation dropdown from `GET /api/workflow/deviations/ready-for-report?reportedByUserId={id}`. Before saving: `var attempt = await workflow.EnsureCanSubmitReportAsync(deviationId);` and store it in `AttemptNumber`. Fill `RootCause` and `CorrectiveAction` (new fields). Example: `SubmitReportAsync`.

### Jayanith — Supervisor Deviations Review
> Before deciding: `WorkflowService.EnsureNotOwnItem(deviation.CreatedBy, supervisorId);`. Accept → `DeviationStatus.Investigating`, reject → `DeviationStatus.Rejected`. Add an `ApprovalRecord` (`ItemTypes.Deviation`). Example: `ReviewDeviationAsync`.

### Manikanta — Supervisor Review Report
> I connected your `ReviewComment` and `ReviewProof`: they are now in `QMSSystem.Shared.Models`, linked by `DeviationReportId` (not `ReviewReportId`), and saved by `QmsDbContext`. Please don't use `ReviewApproval`: the decision goes in `ApprovalRecord` (`ItemTypes.Report`). Before deciding: `WorkflowService.EnsureNotOwnItem(report.CreatedBy, supervisorId);`. On accept, ask the supervisor "Change required? Yes/No", set `ReportStatus.Accepted`, then `await workflow.OnReportAcceptedAsync(report.Id, changeRequired, supervisorId);`. Example: `ReviewReportAsync`.

### Nishanth — Change Requests Index
> List change requests with `Status` from `ChangeRequestStatuses` (Draft, Submitted, Approved, Rejected). Show the linked deviation and document (`DeviationId`, `DocumentId`).

### Gopi — Change Requests Edit
> Fill the report dropdown from `GET /api/workflow/reports/ready-for-change-request`. Before saving: `var link = await workflow.EnsureCanRaiseChangeRequestAsync(reportId);` then copy `link.DeviationId`, `link.DocumentId`, `link.DeviationReportId`. `ChangeType` uses `ChangeTypes`. On Submit: set `ChangeRequestStatuses.Submitted`, then `await workflow.OnChangeRequestSubmittedAsync(changeRequest.Id, operatorId);`. Example: `RaiseChangeRequestAsync`, `SubmitChangeRequestAsync`.

### Rafeeq — Change Request Details (Operator)
> Show the change request with its linked deviation, report and document. Read-only; you can also link to Harshitha's deviation timeline.

### Charan — Change Requests Approve (Supervisor)
> Please don't use `ChangeRequestApproval`: the decision goes in `ApprovalRecord` (`ItemTypes.ChangeRequest`, `Decisions.Accepted` / `Decisions.Rejected`). Before deciding: `WorkflowService.EnsureNotOwnItem(changeRequest.RequestedByUserId, supervisorId);`. Approve → `ChangeRequestStatuses.Approved` then `OnChangeRequestApprovedAsync`; reject → `ChangeRequestStatuses.Rejected` then `OnChangeRequestRejectedAsync`. Example: `DecideChangeRequestAsync`.

### Yash — lead
> 1. Please install the Claude GitHub App on the repo (https://github.com/apps/claude/installations/select_target) so integration work can be pushed from Claude sessions.
> 2. Please turn on "Require review from Code Owners" for `master` (Settings → Branches) so `.github/CODEOWNERS` takes effect.
> 3. Please merge PRs in the order: Documents → Deviations → Reports → Change Requests.
