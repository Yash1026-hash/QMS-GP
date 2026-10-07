namespace QMS.API.Models
{
    public class ReviewComment
    {
        public int Id { get; set; }

        public int ReviewReportId { get; set; }

        public string Comment { get; set; } = string.Empty;

        public string CommentedBy { get; set; } = string.Empty;

        public DateTime CommentDate { get; set; }
    }
}