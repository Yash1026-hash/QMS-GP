using QMSSystem.Shared.Enums;

namespace QMSSystem.Shared.Dtos.Deviations;

public class CreateDeviationRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public Priority Priority { get; set; }
}