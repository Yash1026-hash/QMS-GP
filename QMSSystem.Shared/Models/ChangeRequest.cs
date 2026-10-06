namespace QMSSystem.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class ChangeRequest
{
    public int Id { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int DeviationId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    public string ChangeType { get; set; }   

    [Required]
    public string Description { get; set; }

    [Required]
    public int RequestedByUserId { get; set; }

    [Required]
    public DateTime RequestedDate { get; set; }
}