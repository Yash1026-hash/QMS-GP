namespace QMSSystem.Shared.Dtos;

using System.ComponentModel.DataAnnotations;

public class ChangeRequest
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string ChangeType { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int RequestedByUserId { get; set; }

    [Required]
    public DateTime RequestedDate { get; set; }
    
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public int ApprovedByUserId { get; set; }

    [Required]
    public DateTime ApprovedDate { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}