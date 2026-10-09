namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationRequestDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int DocumentId { get; set; }

    public string Priority { get; set; } = string.Empty;

    // 0 = Pending, 1 = Active, 2 = Inactive
    public int Status { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    // 1 = Approve, 2 = Reject
    public int? Decision { get; set; }

    public string? DecisionBy { get; set; } 

    public DateTime? DecisionOn { get; set; }

    public string ?DecisionComments { get; set; }
}