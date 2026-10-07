// using Microsoft.EntityFrameworkCore;
// using QMSSystem.Api.Data;
// using QMSSystem.Shared.Dtos.Workflow;
// using QMSSystem.Shared.Enums;
// using QMSSystem.Shared.Workflow;

// namespace QMSSystem.Api.Services.Workflow;

// // Read-only lists that connect the pages: what the next step can pick from
// // (the "pickers"), and the full history of one deviation (the timeline).
// public sealed class WorkflowQueries(QmsDbContext context, UserDbContext users)
// {
//     // Operator home page: documents a deviation can be raised against.
//     public async Task<List<ActiveDocumentDto>> GetActiveDocumentsAsync() =>
//         await context.Documents
//             .AsNoTracking()
//             .Where(document => document.Status == DocumentStatuses.Approved)
//             .OrderBy(document => document.DocumentNumber)
//             .Select(document => new ActiveDocumentDto(
//                 document.Id,
//                 document.DocumentNumber,
//                 document.Title,
//                 document.Department,
//                 document.CurrentVersion))
//             .ToListAsync();

//     // Submit Report page: accepted deviations that still need a report.
//     // Pass reportedByUserId to show only one operator's deviations.
//     public async Task<List<DeviationReadyForReportDto>> GetDeviationsReadyForReportAsync(int? reportedByUserId = null)
//     {
//         var query = context.Deviations
//             .AsNoTracking()
//             .Where(deviation =>
//                 deviation.Status == DeviationStatus.Investigating &&
//                 !deviation.Reports.Any(report =>
//                     report.Status == ReportStatus.Pending || report.Status == ReportStatus.Accepted));

//         if (reportedByUserId is int userId)
//         {
//             var createdBy = userId.ToString();
//             query = query.Where(deviation => deviation.CreatedBy == createdBy);
//         }

//         return await (
//             from deviation in query
//             join document in context.Documents on deviation.DocumentId equals document.Id
//             orderby deviation.Id
//             select new DeviationReadyForReportDto(
//                 deviation.Id,
//                 deviation.Title,
//                 document.Id,
//                 document.DocumentNumber,
//                 (deviation.Reports.Max(report => (int?)report.AttemptNumber) ?? 0) + 1))
//             .ToListAsync();
//     }

//     // Change Request Edit page: accepted reports that need a change and have no active request.
//     public async Task<List<ReportReadyForChangeRequestDto>> GetReportsReadyForChangeRequestAsync() =>
//         await (
//             from report in context.DeviationReports.AsNoTracking()
//             join deviation in context.Deviations on report.DeviationId equals deviation.Id
//             join document in context.Documents on deviation.DocumentId equals document.Id
//             where report.Status == ReportStatus.Accepted &&
//                   report.ChangeRequired == true &&
//                   deviation.Status == DeviationStatus.Investigating &&
//                   !context.ChangeRequests.Any(changeRequest =>
//                       changeRequest.DeviationReportId == report.Id &&
//                       changeRequest.Status != ChangeRequestStatuses.Rejected)
//             orderby report.Id
//             select new ReportReadyForChangeRequestDto(
//                 report.Id,
//                 deviation.Id,
//                 deviation.Title,
//                 document.Id,
//                 document.DocumentNumber,
//                 report.AttemptNumber,
//                 report.RootCause,
//                 report.CorrectiveAction))
//             .ToListAsync();

//     // Upload Revision page: approved change requests for this document that
//     // do not have a pending or approved revision yet.
//     public async Task<List<ChangeRequestReadyForRevisionDto>> GetChangeRequestsReadyForRevisionAsync(int documentId) =>
//         await context.ChangeRequests
//             .AsNoTracking()
//             .Where(changeRequest =>
//                 changeRequest.DocumentId == documentId &&
//                 changeRequest.Status == ChangeRequestStatuses.Approved &&
//                 !context.DocumentRevisions.Any(revision =>
//                     revision.ChangeRequestId == changeRequest.Id &&
//                     (revision.ApprovalStatus == RevisionStatuses.Pending ||
//                      revision.ApprovalStatus == RevisionStatuses.Approved)))
//             .OrderBy(changeRequest => changeRequest.Id)
//             .Select(changeRequest => new ChangeRequestReadyForRevisionDto(
//                 changeRequest.Id,
//                 changeRequest.Title,
//                 changeRequest.DocumentId,
//                 changeRequest.DeviationId))
//             .ToListAsync();

//     // Deviation Details page: every step of one deviation, oldest first.
//     // Returns null when the deviation does not exist.
//     public async Task<List<TimelineEntryDto>?> GetTimelineAsync(int deviationId)
//     {
//         var deviation = await context.Deviations
//             .AsNoTracking()
//             .Include(item => item.Reports)
//             .FirstOrDefaultAsync(item => item.Id == deviationId);
//         if (deviation is null)
//         {
//             return null;
//         }

//         var reportIds = deviation.Reports.Select(report => report.Id).ToList();
//         // var changeRequests = await context.ChangeRequests
//         //     .AsNoTracking()
//         //     .Where(changeRequest => changeRequest.DeviationId == deviationId)
//         //     .ToListAsync();
//         // var changeRequestIds = changeRequests.Select(changeRequest => changeRequest.Id).ToList();
//         var revisions = await context.DocumentRevisions
//             .AsNoTracking()
//             .Where(revision => revision.ChangeRequestId != null && changeRequestIds.Contains(revision.ChangeRequestId.Value))
//             .ToListAsync();
//         var decisions = await context.ApprovalRecords
//             .AsNoTracking()
//             .Where(record =>
//                 (record.ItemType == ItemTypes.Deviation && record.ItemId == deviationId) ||
//                 (record.ItemType == ItemTypes.Report && reportIds.Contains(record.ItemId)) ||
//                 (record.ItemType == ItemTypes.ChangeRequest && changeRequestIds.Contains(record.ItemId)))
//             .ToListAsync();

//         var steps = new List<(DateTime When, string Step, string Detail, string? Who, string ItemType, int ItemId)>
//         {
//             (deviation.CreatedDate, "Deviation raised", $"{deviation.Priority}: {deviation.Title}",
//                 deviation.CreatedBy, ItemTypes.Deviation, deviation.Id)
//         };

//         foreach (var report in deviation.Reports)
//         {
//             steps.Add((report.CreatedDate, $"Report submitted (attempt {report.AttemptNumber})",
//                 report.RootCause, report.CreatedBy, ItemTypes.Report, report.Id));
//         }

//         foreach (var changeRequest in changeRequests)
//         {
//             steps.Add((changeRequest.RequestedDate, $"Change request {changeRequest.Id} raised",
//                 changeRequest.Title, changeRequest.RequestedByUserId.ToString(), ItemTypes.ChangeRequest, changeRequest.Id));
//         }

//         foreach (var revision in revisions)
//         {
//             steps.Add((revision.UploadedTime, $"Revision {revision.Version} uploaded",
//                 revision.ChangeSummary, revision.UploadedBy, ItemTypes.Revision, revision.Id));
//             if (revision.ApprovalDate is DateTime decidedOn && revision.ApprovalStatus != RevisionStatuses.Pending)
//             {
//                 steps.Add((decidedOn, $"Revision {revision.Version} {revision.ApprovalStatus.ToLowerInvariant()}",
//                     string.Empty, revision.ApprovedBy, ItemTypes.Revision, revision.Id));
//             }
//         }

//         foreach (var decision in decisions)
//         {
//             var subject = decision.ItemType switch
//             {
//                 ItemTypes.Deviation => "Deviation",
//                 ItemTypes.Report => $"Report {decision.ItemId}",
//                 _ => $"Change request {decision.ItemId}"
//             };
//             steps.Add((decision.DecisionDate, $"{subject} {decision.Decision.ToLowerInvariant()}",
//                 decision.Comments, decision.ReviewedByUserId.ToString(), decision.ItemType, decision.ItemId));
//         }

//         if (deviation.ClosedDate is DateTime closedOn)
//         {
//             steps.Add((closedOn, "Deviation closed", string.Empty, deviation.ClosedBy, ItemTypes.Deviation, deviation.Id));
//         }

//         var names = await LoadUserNamesAsync(steps.Select(step => step.Who));

//         return steps
//             .OrderBy(step => step.When)
//             .Select(step => new TimelineEntryDto(
//                 step.When,
//                 step.Step,
//                 step.Detail,
//                 NameOf(step.Who, names),
//                 step.ItemType,
//                 step.ItemId))
//             .ToList();
//     }

//     private async Task<Dictionary<int, string>> LoadUserNamesAsync(IEnumerable<string?> userIds)
//     {
//         var ids = userIds
//             .Select(value => int.TryParse(value, out var id) ? id : (int?)null)
//             .OfType<int>()
//             .Distinct()
//             .ToList();
//         if (ids.Count == 0)
//         {
//             return [];
//         }

//         return await users.Users
//             .AsNoTracking()
//             .Where(user => ids.Contains(user.UserId))
//             .ToDictionaryAsync(
//                 user => user.UserId,
//                 user => string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName);
//     }

//     private static string NameOf(string? who, Dictionary<int, string> names) =>
//         int.TryParse(who, out var id) && names.TryGetValue(id, out var name)
//             ? name
//             : who ?? string.Empty;
// }
