// using Microsoft.EntityFrameworkCore;
// using QMSSystem.Shared.Enums;
// using QMSSystem.Shared.Models;
// using QMSSystem.Shared.Workflow;

// namespace QMSSystem.Api.Data;

// // User ids of existing login accounts that the demo records point to.
// // Use two supervisors: a supervisor cannot review an item they raised.
// public sealed record DemoUserIds(int Operator, int SecondOperator, int Supervisor, int SecondSupervisor);

// // Fills an EMPTY database with one record at every step of the workflow,
// // so every teammate can test their page without waiting for the others.
// // It does nothing when documents already exist.
// //
// // Enable it for a local database only: set Qms:SeedDemoData = true and the
// // four Qms:DemoUsers ids in user secrets. Never run it against the shared database.
// public static class DemoDataSeeder
// {
//     public static async Task<bool> SeedAsync(QmsDbContext context, DemoUserIds users, DateTime now)
//     {
//         if (await context.Documents.AnyAsync())
//         {
//             return false;
//         }

//         var day = now.Date.AddDays(-30);
//         var op = users.Operator.ToString();
//         var op2 = users.SecondOperator.ToString();
//         var sup = users.Supervisor.ToString();

//         // ---- Documents
//         // SOP-001: approved, version 1. Most demo deviations are raised against it.
//         // SOP-002: approved, version 2. Version 2 came from the fully closed chain (DEV "Mixer speed").
//         // SOP-003: draft, version 1 waiting for approval.
//         var sop1 = NewDocument("SOP-001", "Sample Incubation Procedure", "Quality", 1, DocumentStatuses.Approved, users.Operator, day);
//         var sop2 = NewDocument("SOP-002", "Mixer Cleaning Procedure", "Production", 2, DocumentStatuses.Approved, users.Operator, day);
//         var sop3 = NewDocument("SOP-003", "Label Printing Procedure", "Packaging", 1, DocumentStatuses.Draft, users.SecondOperator, day.AddDays(25));
//         context.Documents.AddRange(sop1, sop2, sop3);
//         await context.SaveChangesAsync();

//         context.DocumentRevisions.AddRange(
//             NewRevision(sop1, 1, RevisionStatuses.Approved, op, sup, day, "First issue"),
//             NewRevision(sop2, 1, RevisionStatuses.Approved, op, sup, day, "First issue"),
//             NewRevision(sop3, 1, RevisionStatuses.Pending, op2, null, day.AddDays(25), "First issue"));

//         // ---- Deviations, one per workflow step
//         var open = NewDeviation(sop1, "Incubator door left open", Priority.Minor, DeviationStatus.Open, op, day.AddDays(20));
//         var rejected = NewDeviation(sop1, "Duplicate entry for door alarm", Priority.Minor, DeviationStatus.Rejected, op2, day.AddDays(18));
//         var needsReport = NewDeviation(sop1, "Sample tested at 40 C instead of 37 C", Priority.Major, DeviationStatus.Investigating, op, day.AddDays(15));
//         var reportPending = NewDeviation(sop1, "Timer not started on batch 12", Priority.Minor, DeviationStatus.Investigating, op2, day.AddDays(12));
//         var needsChangeRequest = NewDeviation(sop1, "Thermometer calibration expired", Priority.Major, DeviationStatus.Investigating, op, day.AddDays(10));
//         var changeSubmitted = NewDeviation(sop1, "Wrong incubation time in step 4", Priority.Critical, DeviationStatus.AwaitingChange, op, day.AddDays(8));
//         var changeApproved = NewDeviation(sop1, "Missing temperature log sheet", Priority.Major, DeviationStatus.AwaitingChange, op2, day.AddDays(6));
//         var closedNoChange = NewDeviation(sop1, "Gloves not changed between samples", Priority.Minor, DeviationStatus.Closed, op2, day.AddDays(4));
//         var closedFullChain = NewDeviation(sop2, "Mixer speed above limit", Priority.Major, DeviationStatus.Closed, op, day.AddDays(1));
//         context.Deviations.AddRange(open, rejected, needsReport, reportPending, needsChangeRequest,
//             changeSubmitted, changeApproved, closedNoChange, closedFullChain);
//         await context.SaveChangesAsync();

//         foreach (var accepted in new[] { needsReport, reportPending, needsChangeRequest, changeSubmitted, changeApproved, closedNoChange, closedFullChain })
//         {
//             context.ApprovalRecords.Add(NewDecision(ItemTypes.Deviation, accepted.Id, Decisions.Accepted, users.Supervisor, accepted.CreatedDate.AddHours(2), "Valid deviation"));
//         }
//         context.ApprovalRecords.Add(NewDecision(ItemTypes.Deviation, rejected.Id, Decisions.Rejected, users.Supervisor, rejected.CreatedDate.AddHours(2), "Duplicate of an earlier report"));

//         // ---- Reports
//         var firstAttempt = NewReport(reportPending, 1, ReportStatus.Rejected, null, op2, day.AddDays(13), "Operator forgot the timer");
//         var secondAttempt = NewReport(reportPending, 2, ReportStatus.Pending, null, op2, day.AddDays(14), "Timer button is hard to reach");
//         var needsChangeReport = NewReport(needsChangeRequest, 1, ReportStatus.Accepted, true, op, day.AddDays(11), "Calibration date not checked in the SOP");
//         var changeSubmittedReport = NewReport(changeSubmitted, 1, ReportStatus.Accepted, true, op, day.AddDays(9), "SOP step 4 states 12 h, validated time is 18 h");
//         var changeApprovedReport = NewReport(changeApproved, 1, ReportStatus.Accepted, true, op2, day.AddDays(7), "SOP has no log sheet step");
//         var noChangeReport = NewReport(closedNoChange, 1, ReportStatus.Accepted, false, op2, day.AddDays(5), "One-time mistake, operator retrained");
//         var fullChainReport = NewReport(closedFullChain, 1, ReportStatus.Accepted, true, op, day.AddDays(2), "Speed limit missing from SOP-002");
//         context.DeviationReports.AddRange(firstAttempt, secondAttempt, needsChangeReport, changeSubmittedReport,
//             changeApprovedReport, noChangeReport, fullChainReport);
//         await context.SaveChangesAsync();

//         context.ApprovalRecords.AddRange(
//             NewDecision(ItemTypes.Report, firstAttempt.Id, Decisions.Rejected, users.Supervisor, day.AddDays(13).AddHours(3), "Root cause is not clear"),
//             NewDecision(ItemTypes.Report, needsChangeReport.Id, Decisions.Accepted, users.Supervisor, day.AddDays(11).AddHours(3), "Change required"),
//             NewDecision(ItemTypes.Report, changeSubmittedReport.Id, Decisions.Accepted, users.Supervisor, day.AddDays(9).AddHours(3), "Change required"),
//             NewDecision(ItemTypes.Report, changeApprovedReport.Id, Decisions.Accepted, users.Supervisor, day.AddDays(7).AddHours(3), "Change required"),
//             NewDecision(ItemTypes.Report, noChangeReport.Id, Decisions.Accepted, users.Supervisor, day.AddDays(5).AddHours(3), "No change needed"),
//             NewDecision(ItemTypes.Report, fullChainReport.Id, Decisions.Accepted, users.Supervisor, day.AddDays(2).AddHours(3), "Change required"));

//         // ---- Change requests
//         var submittedRequest = NewChangeRequest(changeSubmittedReport, changeSubmitted, "Change step 4 incubation time to 18 h", ChangeTypes.Process, ChangeRequestStatuses.Submitted, users.Operator, day.AddDays(9).AddHours(5));
//         var approvedRequest = NewChangeRequest(changeApprovedReport, changeApproved, "Add a temperature log sheet step", ChangeTypes.Process, ChangeRequestStatuses.Approved, users.SecondOperator, day.AddDays(7).AddHours(5));
//         var implementedRequest = NewChangeRequest(fullChainReport, closedFullChain, "Add mixer speed limit to SOP-002", ChangeTypes.Equipment, ChangeRequestStatuses.Approved, users.Operator, day.AddDays(2).AddHours(5));
//         context.ChangeRequests.AddRange(submittedRequest, approvedRequest, implementedRequest);
//         await context.SaveChangesAsync();

//         context.ApprovalRecords.AddRange(
//             NewDecision(ItemTypes.ChangeRequest, approvedRequest.Id, Decisions.Accepted, users.Supervisor, day.AddDays(7).AddHours(8), "Approved"),
//             NewDecision(ItemTypes.ChangeRequest, implementedRequest.Id, Decisions.Accepted, users.Supervisor, day.AddDays(2).AddHours(8), "Approved"));

//         var implementingRevision = NewRevision(sop2, 2, RevisionStatuses.Approved, op, users.SecondSupervisor.ToString(), day.AddDays(3), "Added mixer speed limit");
//         implementingRevision.ChangeRequestId = implementedRequest.Id;
//         context.DocumentRevisions.Add(implementingRevision);

//         closedNoChange.ClosedBy = sup;
//         closedNoChange.ClosedDate = day.AddDays(5).AddHours(3);
//         closedFullChain.ClosedBy = users.SecondSupervisor.ToString();
//         closedFullChain.ClosedDate = day.AddDays(3).AddHours(4);

//         await context.SaveChangesAsync();
//         return true;
//     }

//     private static Document NewDocument(string number, string title, string department, int version, string status, int createdBy, DateTime created) => new()
//     {
//         DocumentNumber = number,
//         Title = title,
//         Department = department,
//         CurrentVersion = version,
//         Status = status,
//         CreatedBy = createdBy,
//         CreationTime = created
//     };

//     private static DocumentRevision NewRevision(Document document, int version, string status, string uploadedBy, string? approvedBy, DateTime uploaded, string summary) => new()
//     {
//         Document = document,
//         Version = version,
//         FileName = $"{document.DocumentNumber}-v{version}.pdf",
//         UploadedBy = uploadedBy,
//         UploadedTime = uploaded,
//         ChangeSummary = summary,
//         ApprovalStatus = status,
//         ApprovedBy = approvedBy,
//         ApprovalDate = status == RevisionStatuses.Pending ? null : uploaded.AddHours(4)
//     };

//     private static Deviation NewDeviation(Document document, string title, Priority priority, DeviationStatus status, string createdBy, DateTime created) => new()
//     {
//         DocumentId = document.Id,
//         Title = title,
//         Description = title,
//         Priority = priority,
//         Status = status,
//         CreatedBy = createdBy,
//         CreatedDate = created
//     };

//     private static DeviationReport NewReport(Deviation deviation, int attempt, ReportStatus status, bool? changeRequired, string createdBy, DateTime created, string rootCause) => new()
//     {
//         DeviationId = deviation.Id,
//         AttemptNumber = attempt,
//         Status = status,
//         ChangeRequired = changeRequired,
//         Summary = rootCause,
//         RootCause = rootCause,
//         CorrectiveAction = changeRequired == true ? "Update the SOP" : "Retrain the operator",
//         FileName = $"report-{deviation.Id}-{attempt}.pdf",
//         CreatedBy = createdBy,
//         CreatedDate = created
//     };

//     private static ChangeRequest NewChangeRequest(DeviationReport report, Deviation deviation, string title, string changeType, string status, int requestedBy, DateTime requested) => new()
//     {
//        DocumentId = deviation.DocumentId,
//         Title = title,
//         Description = title,
//         ChangeType = changeType,
//         RequestedByUserId = requestedBy,
//         RequestedDate = requested
//     };

//     private static ApprovalRecord NewDecision(string itemType, int itemId, string decision, int reviewer, DateTime decided, string comments) => new()
//     {
//         ItemType = itemType,
//         ItemId = itemId,
//         Decision = decision,
//         ReviewedByUserId = reviewer,
//         DecisionDate = decided,
//         Comments = comments
//     };
// }
