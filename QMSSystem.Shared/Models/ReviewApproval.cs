namespace QMS.API.Models
{
    public class ReviewApproval
    {
        public int Id { get; set; }

        public int ReviewReportId { get; set; }

        public string ApprovalStatus { get; set; } = "Pending";

        public string ApprovedBy { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;

        public DateTime? ApprovalDate { get; set; }
    }
}