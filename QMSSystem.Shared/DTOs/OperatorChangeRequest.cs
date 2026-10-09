namespace QMSSystem.Shared.DTOs;

using System.ComponentModel.DataAnnotations;

public class OperatorChangeRequest
{
    [Key]
    public int Id { get; set; }
     
   
    public int DocumentId { get; set; }
     
   
    [StringLength(200)]
    public string? Title { get; set; } 
     
    
    [StringLength(50)]
    public string? ChangeType { get; set; } 
     
    
    public string? Description { get; set; } 
     
    
    public int? RequestedByUserId { get; set; }
     
   
    public DateTime RequestedDate { get; set; }

    public string? Decision { get; set; }  //accept,reject
    
    [StringLength(1000)]
    public string? DecisionComment { get; set; }

    
    public int? DecisionByUserId { get; set; }

    public DateTime? DecisionDate { get; set; }

    
    public string? Status { get; set; }  //active,inactive,pending

}