using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationReportDto
{
    public int Id { get; set; }

    public int DeviationId { get; set; }

    public int AttemptNumber { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public ReportStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }
}