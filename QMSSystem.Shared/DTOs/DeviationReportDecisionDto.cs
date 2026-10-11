namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationReportDecisionDto
{
    // 1 = Approve, 2 = Reject
    public int Decision { get; set; }

    public string DecisionComments { get; set; } = string.Empty;
}
