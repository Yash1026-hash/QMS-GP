namespace QMSSystem.Shared.Dtos.Documents;

/// <summary>Represents a document available for selection when creating a revision.</summary>
public sealed record DocumentOptionResponse(
    int Id,
    string DocumentNumber,
    string Title);
