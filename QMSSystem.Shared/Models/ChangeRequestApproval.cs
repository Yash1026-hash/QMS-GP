namespace QMSSystem.Shared.Models;

using System.ComponentModel.DataAnnotations;

public class ChangeRequestApproval
{
    // public int Id { get; set; }
    [Key]
    [Required]
    public int ChangeRequestId { get; set; }

    [Required]
    public string Decision { get; set; } = string.Empty; //accept,reject
    
    [StringLength(1000)]
    public string DecisionComment { get; set; } = string.Empty;

    [Required]
    public int DecisionByUserId { get; set; }

    [Required]
    public DateTime DecisionDate { get; set; }

}