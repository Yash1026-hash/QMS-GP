
using System.ComponentModel.DataAnnotations.Schema;

//document model
public class DocumentCreation
{
    public int Id { get; set; }

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

    [NotMapped]
    public string? CreatedByName { get; set; }

    public string? Comment { get; set; }

    [Column("decisionBy", TypeName = "varchar")]
    public string? DecisionBy { get; set; }

    [Column("decisionDate", TypeName = "datetime")]
    public DateTime? DecisionDate { get; set; }

    [Column("decision", TypeName = "int")]
    public int? DecisionStatus { get; set; }

    [NotMapped]
    public int? Decision
    {
        get => DecisionStatus;
        set => DecisionStatus = value;
    }
}