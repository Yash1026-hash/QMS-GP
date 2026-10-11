namespace QMSSystem.Shared.Models;

public class UserRole
{
    public int UserId { get; set; }
    public UserAccount User { get; set; } = null!;
    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
}