namespace QMSSystem.Shared.DTOs;

public class DocumentHistoryDto
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int DocumentVersion { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;         // ← ADD
    public string ContentType { get; set; } = string.Empty;      // ← ADD
    public int CreatedBy { get; set; }
    public DateTime CreationOn { get; set; }
    public string? Comment { get; set; }
    public int? DecisionBy { get; set; }
    public string? Decision { get; set; }
    public DateTime? DecisionDate { get; set; }
    public int DecisionStatus { get; set; }
    public DateTime ArchivedOn { get; set; }                     // ← ADD
    public int ArchivedBy { get; set; }                          // ← ADD
}