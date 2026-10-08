namespace QMSSystem.Shared.Models;

using QMSSystem.Shared.Dtos;

public class UserRole
{
    public int UserId { get; set; }
    public UserDto User { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}