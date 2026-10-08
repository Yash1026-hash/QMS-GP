namespace QMSSystem.Shared.DTOs;

using System.ComponentModel.DataAnnotations;

public class OperatorChangeRequest
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

    public string Decision { get; set; } = string.Empty; //accept,reject
    
    [StringLength(1000)]
    public string DecisionComment { get; set; } = string.Empty;

    [Required]
    public int DecisionByUserId { get; set; }

    [Required]
    public DateTime DecisionDate { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;  //active,inactive,pending

}