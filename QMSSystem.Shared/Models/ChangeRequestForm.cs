namespace QMSSystem.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class ChangeRequestForm
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
    public int Status {get;set;}
    public List<int> SelectedDeviationIds { get; set; } = [];
     
    [Required]
    public int RequestedByUserId { get; set; }
     
    [Required]
    public DateTime RequestedDate { get; set; }
}