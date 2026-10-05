namespace VehicleRecall.Shared.Models;

public sealed record LoginResponse(
    int UserId,
    string Username,
    string Role,
    string FullName,
    string Email,
    string AccessToken,
    DateTimeOffset ExpiresAt)
{
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
