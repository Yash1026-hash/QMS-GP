namespace QMSSystem.Shared.Models;

public class DeviationAttachment : BaseEntity
{
    public int DeviationId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredPath { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}