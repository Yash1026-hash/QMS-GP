namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationReportDto
{
    public int Id { get; set; }

    public int DeviationId { get; set; }

    public int DocumentId { get; set; }

    public int AttemptNumber { get; set; }

    public string Summary { get; set; } = string.Empty;

    // Name/path of the uploaded proof file
    public byte[]? Proof { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    // 0 = Pending, 1 = Approved, 2 = Rejected
    public int Status { get; set; }

    // 1 = Approve, 2 = Reject
   public int? Decision { get; set; }

    public string DecisionBy { get; set; } = string.Empty;

    public DateTime? DecisionOn { get; set; }

    public string DecisionComments { get; set; } = string.Empty;


    
}