 namespace QMSSystem.Shared.Models;

public class DocumentHistory
{
    public int Id { get; set; }

    // Which document this snapshot belongs to
    public int DocumentId { get; set; }

    // Snapshot fields — exact copy of Document at time of change
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int DocumentVersion { get; set; }
    public string Status { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] FileData { get; set; } = [];

    public int CreatedBy { get; set; }
    public DateTime CreationOn { get; set; }
    public string? Comment { get; set; }

    // When was this snapshot archived
    public DateTime ArchivedOn { get; set; }
    public int ArchivedBy { get; set; }

    // Navigation
    public Document? Document { get; set; }
}