using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationDetailsDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public Priority Priority { get; set; }

    public DeviationStatus Status { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public List<DeviationAttachmentDto> Attachments { get; set; }
        = new();

    public List<DeviationReportDto> Reports { get; set; }
        = new();
}