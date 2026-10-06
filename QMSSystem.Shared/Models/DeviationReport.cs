using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Models;

public class DeviationReport : BaseEntity
{
    public int DeviationId { get; set; }

    public int AttemptNumber { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public Deviation? Deviation { get; set; }
}