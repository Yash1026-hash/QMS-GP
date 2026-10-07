namespace QMSSystem.Shared.Models
{
    // A supervisor comment on a deviation report, written while reviewing it.
    public class ReviewComment
    {
        public int Id { get; set; }

        // The report being reviewed (KS_DeviationReports.Id).
        public int DeviationReportId { get; set; }

        public string Comment { get; set; } = string.Empty;

        // UserId of the supervisor, as text (same as Deviation.CreatedBy).
        public string CommentedBy { get; set; } = string.Empty;

        public DateTime CommentDate { get; set; }
    }
}
