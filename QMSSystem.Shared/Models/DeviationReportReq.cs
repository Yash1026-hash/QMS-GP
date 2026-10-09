namespace QMSSystem.Shared.Models;

public class DeviationReportRequest
{
    public int Id { get; set; }

    public int DeviationId { get; set; }

    public int DocumentId { get; set; }

    public int AttemptNumber { get; set; } = 1;

    public string Summary { get; set; } = string.Empty;

    // Uploaded proof file name/path

    public byte[]? Proof { get; set; }
    
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // 0 = Pending, 1 = Active, 2 = Inactive
    public int Status { get; set; } = 0;

    // 1 = Approve, 2 = Reject
    public int? Decision { get; set; }

    public string? DecisionBy { get; set; }

    public DateTime? DecisionOn { get; set; }

    public string? DecisionComments { get; set; }
}