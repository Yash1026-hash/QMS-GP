namespace QMS.API.Models
{
    public class ReviewFinding
    {
        public int Id { get; set; }

        public int ReviewReportId { get; set; }

        public string FindingTitle { get; set; } = string.Empty;

        public string FindingDescription { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Recommendation { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";
    }
}