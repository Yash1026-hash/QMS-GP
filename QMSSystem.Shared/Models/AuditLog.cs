namespace QMSSystem.Shared.Models;

// Append-only record of every action. Never update or delete rows.
// DeviationId is set whenever the action belongs to a deviation's chain,
// so the full history of one deviation comes from one query.
public class AuditLog
{
    public int Id { get; set; }

    public string ItemType { get; set; } = string.Empty;

    public int ItemId { get; set; }

    public int? DeviationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public int? ActorUserId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string Comment { get; set; } = string.Empty;
}
