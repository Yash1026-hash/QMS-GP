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
    public string Title { get; set; }= string.Empty;

    [Required]
    public string ChangeType { get; set; }= string.Empty;

    [Required]
    public string Description { get; set; }= string.Empty;

    [Required]
    public int RequestedByUserId { get; set; }

    [Required]
    public DateTime RequestedDate { get; set; }

    // Integration fields. Use the values in QMSSystem.Shared.Workflow.ChangeRequestStatuses.
    [StringLength(20)]
    public string Status { get; set; } = Workflow.ChangeRequestStatuses.Draft;

    // The accepted deviation report this change request was raised from.
    public int? DeviationReportId { get; set; }
}