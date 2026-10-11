using System.ComponentModel.DataAnnotations;

public class DocumentApproval
{
    
    public int Id { get; set; }
    public string? Comments { get; set; }
    public string? DecisionBy { get; set; }
    public DateTime? DecisionDate { get; set; }
    public int DecisionStatus {get; set;}
    
    
}