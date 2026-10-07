using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Models;

public class DeviationReport : BaseEntity
{
    public int DeviationId { get; set; }

    public int AttemptNumber { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string RootCause { get; set; } = string.Empty;

    public string CorrectiveAction { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    // Investigation fields required by the Deviation problem statement.

    // Set by the supervisor when the report is accepted:
    // true = a change request is needed, false = the deviation can close now.
    public bool? ChangeRequired { get; set; }

    public Deviation? Deviation { get; set; }
}