
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

    public string? Comment { get; set; }
<<<<<<< HEAD
    public string? DecisionBy { get; set; }
    public DateTime? DecisionDate { get; set; }
    public int? Decision { get; set; }
=======

    [Column("decisionBy", TypeName = "varchar")]
    public string? DecisionBy { get; set; }

    [Column("decisionDate", TypeName = "datetime")]
    public DateTime? DecisionDate { get; set; }

    [Column("decision", TypeName = "int")]
    public int? DecisionStatus { get; set; }
>>>>>>> 9f4b6723144ddfe6d70b12b01a8b1c444fa9075f
}