namespace QMSSystem.Shared.Models;

// One supervisor decision on one item. One table for all review types.
// ItemType uses QMSSystem.Shared.Workflow.ItemTypes and Decision uses Decisions.
public class ApprovalRecord
{
    public int Id { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public string Decision { get; set; } = string.Empty;

    public string Comments { get; set; } = string.Empty;

    public int ReviewedByUserId { get; set; }

    public DateTime DecisionDate { get; set; } = DateTime.UtcNow;
}
