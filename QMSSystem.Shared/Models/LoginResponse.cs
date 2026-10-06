namespace QMSSystem.Shared.Models;

public sealed record LoginResponse(
    int UserId,
    string Username,
    string Role,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles);
