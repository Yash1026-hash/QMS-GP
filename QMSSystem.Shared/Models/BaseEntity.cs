namespace QMSSystem.Shared.Models;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}