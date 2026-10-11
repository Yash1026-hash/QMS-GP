using System.ComponentModel.DataAnnotations.Schema;

namespace QMSSystem.Shared.Models;

public class DocumentEvent
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int ActorUserId { get; set; }
    public DateTime EventOn { get; set; }
    public string? Comment { get; set; }

    [NotMapped]
    public string? ActorName { get; set; }
}
