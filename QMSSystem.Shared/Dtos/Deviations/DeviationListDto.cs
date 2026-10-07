using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationListDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public Priority Priority { get; set; }

    public DeviationStatus Status { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string? SupervisorComment { get; set; }
}