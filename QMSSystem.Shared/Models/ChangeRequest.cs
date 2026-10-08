namespace QMSSystem.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class ChangeRequest
{
    public int Id { get; set; }

    [Required]
    public int ChangeRequestId { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public int ApprovedByUserId { get; set; }

    [Required]
    public DateTime ApprovedDate { get; set; }
}