namespace QMSSystem.Shared.Dtos.Workflow;

// Data returned by /api/workflow endpoints. Pages use these to fill
// dropdowns ("pickers") and to show the traceability timeline.

public sealed record ActiveDocumentDto(
    int Id,
    string DocumentNumber,
    string Title,
    string Department,
    int CurrentVersion);

public sealed record DeviationReadyForReportDto(
    int DeviationId,
    string Title,
    int DocumentId,
    string DocumentNumber,
    int NextAttemptNumber);

public sealed record ReportReadyForChangeRequestDto(
    int ReportId,
    int DeviationId,
    string DeviationTitle,
    int DocumentId,
    string DocumentNumber,
    int AttemptNumber,
    string RootCause,
    string CorrectiveAction);

public sealed record ChangeRequestReadyForRevisionDto(
    int ChangeRequestId,
    string Title,
    int DocumentId,
    int DeviationId);

public sealed record TimelineEntryDto(
    DateTime When,
    string Step,
    string Detail,
    string Who,
    string ItemType,
    int ItemId);
