namespace QMSSystem.Shared.Models;

public class DeviationReportApproval
{

    public int DeviationReportRequestId { get; set; }

    // 1 = Approve, 2 = Reject
    public int Decision { get; set; }

    public string DecisionBy { get; set; } = string.Empty;

    public DateTime DecisionOn { get; set; }

    public string DecisionComments { get; set; } = string.Empty;

}