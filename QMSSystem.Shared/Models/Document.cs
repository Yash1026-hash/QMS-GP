namespace QMSSystem.Shared.Models;

public class Document
{
    public int Id { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public int CurrentVersion { get; set; }

    public string Status { get; set; } = string.Empty;

    public int CreatedBy { get; set; }

    public DateTime CreationTime { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public byte[] FileData { get; set; } = [];

    public string? Comment { get; set; }
}