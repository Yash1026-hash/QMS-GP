namespace QMSSystem.Shared.Enums;

public enum DeviationStatus
{
    Open,
    Investigating,
    Closed,
    Rejected,

    // Added for integration: a change request was submitted for this deviation
    // and the deviation waits for the change and the new SOP revision.
    // Added at the end so the existing numeric values do not change.
    AwaitingChange
}