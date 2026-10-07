namespace QMSSystem.Shared.Models
{
    public class DocumentRevision
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int Version { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = [];
        public string? Comment { get; set; }
        public string UploadedBy { get; set; } = string.Empty;
        public DateTime UploadedTime { get; set; }
        public string ChangeSummary { get; set; } = string.Empty;
        public string ApprovalStatus { get; set; } = string.Empty;
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }

        // Integration field: the approved change request that this revision implements.
        // Empty for version 1 of a new document.
        public int? ChangeRequestId { get; set; }

        public Document Document { get; set; } = null!;
    }
}
