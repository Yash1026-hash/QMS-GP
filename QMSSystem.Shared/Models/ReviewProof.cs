namespace QMS.API.Models
{
    public class ReviewProof
    {
        public int Id { get; set; }

        public int ReviewReportId { get; set; }

        public int? FindingId { get; set; }

        public string ProofName { get; set; } = string.Empty;

        public string ProofType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}