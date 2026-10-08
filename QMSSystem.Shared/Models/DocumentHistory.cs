 namespace QMSSystem.Shared.Models;

public class DocumentHistory
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
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
    public DateTime ArchivedOn { get; set; }
    public int ArchivedBy { get; set; }
}