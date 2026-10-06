namespace QMSSystem.Shared.Dtos.Deviations;

public class DeviationAttachmentDto
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
}