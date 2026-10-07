namespace QMSSystem.Shared.Workflow;

// The official status words for the fields that are stored as text.
// Always use these constants. Never type the words by hand, because
// "Approved" and "approved" or "Accepted" are different values in the database.

public static class DocumentStatuses
{
    public const string Draft = "Draft";
    public const string Approved = "Approved";
    public const string Obsolete = "Obsolete";
}

public static class RevisionStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class ChangeRequestStatuses
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public static class ChangeTypes
{
    public const string Process = "Process";
    public const string Equipment = "Equipment";
    public const string Software = "Software";
}

// The kind of item an ApprovalRecord or an AuditLog row is about.
public static class ItemTypes
{
    public const string Document = "Document";
    public const string Revision = "Revision";
    public const string Deviation = "Deviation";
    public const string Report = "Report";
    public const string ChangeRequest = "ChangeRequest";
}

public static class Decisions
{
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
}
