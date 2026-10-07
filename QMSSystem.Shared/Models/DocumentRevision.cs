namespace QMSSystem.Shared.Models
{
    public class DocumentRevision
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int Version { get; set; }
        public string FilePath {get; set;}=string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedTime { get; set; }
        public string ChangeSummary { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string? ReviewComment { get; set; }
        public int? ChangeRequestId { get; set; }

        public Document Document { get; set; } = null!;
    }
}

