namespace QMS.API.Models
{
    public class ReviewReport
    {
        public int Id { get; set; }

        public int DeviationReportId { get; set; }

        public string ReviewNumber { get; set; } = string.Empty;

        public string ReviewTitle { get; set; } = string.Empty;

        public string ReviewSummary { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string ReviewedBy { get; set; } = string.Empty;

        public DateTime ReviewDate { get; set; }

        public DateTime? CompletedDate { get; set; }
    }
}