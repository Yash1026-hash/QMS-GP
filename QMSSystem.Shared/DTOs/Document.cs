namespace QMSSystem.Shared.Models;


//document dto
public class Document
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

    public string? CreatedByName { get; set; }

    public DateTime CreationOn { get; set; }

  
    public string? Comment { get; set; }

   
  
    public string? DecisionBy { get; set; }

    public int? Decision { get; set; }

    public string? DecisionText { get; set; }

    public int? DecisionStatus
    {
        get => Decision;
        set => Decision = value;
    }

    public DateTime? DecisionDate { get; set; }
}