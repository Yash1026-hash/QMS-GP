using QMSSystem.Api.Data;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Services.Workflow;

// Adds a row to the append-only audit log. The row is saved by the caller's
// next SaveChanges, so the action and its audit row are saved together.
public sealed class AuditService(QmsDbContext context, TimeProvider clock)
{
    public void Record(
        string itemType,
        int itemId,
        string action,
        int? actorUserId,
        int? deviationId = null,
        string comment = "")
    {
        context.AuditLogs.Add(new AuditLog
        {
            ItemType = itemType,
            ItemId = itemId,
            DeviationId = deviationId,
            Action = action,
            ActorUserId = actorUserId,
            Timestamp = clock.GetUtcNow().UtcDateTime,
            Comment = comment
        });
    }
}
