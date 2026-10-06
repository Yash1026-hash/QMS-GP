using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Models;

public class Deviation : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public Priority Priority { get; set; }

    public DeviationStatus Status { get; set; } = DeviationStatus.Open;

    public ICollection<DeviationAttachment> Attachments { get; set; }
        = new List<DeviationAttachment>();

    public ICollection<DeviationReport> Reports { get; set; }
        = new List<DeviationReport>();
}