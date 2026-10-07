namespace QMSSystem.Shared.Models
{
    // A proof file attached to a deviation report review.
    public class ReviewProof
    {
        public int Id { get; set; }

        // The report being reviewed (KS_DeviationReports.Id).
        public int DeviationReportId { get; set; }

        public int? FindingId { get; set; }

        public string ProofName { get; set; } = string.Empty;

        public string ProofType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }
    }
}
