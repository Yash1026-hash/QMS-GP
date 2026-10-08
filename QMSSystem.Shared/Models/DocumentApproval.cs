using System.ComponentModel.DataAnnotations;

public class DocumentApproval
{
    
    public int Id { get; set; }
    public string ApprovalStatus { get; set; } = "Pending";
    public string? Comments { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public int ApproverDescision {get; set;}
    
    
}