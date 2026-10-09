using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.Models;
 
namespace QMSSystem.Api.Services;
 
public sealed class UserStore(
    UserDbContext context,
    IPasswordHasher<UserDto> passwordHasher)
{
    private const string PasswordHashPrefix = "hash$v1$";
 
    public UserDto? ValidateCredentials(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();
        var account = context.Users
            .Include(account => account.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(account => account.Username.ToUpper() == normalizedUsername);
 
        if (account is null ||
            !account.IsActive ||
            !string.Equals(account.RegistrationStatus, "Registered", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }
 
        var user = ToUserDto(account);
 
        if (account.Password.StartsWith(PasswordHashPrefix, StringComparison.Ordinal))
        {
            var verification = passwordHasher.VerifyHashedPassword(
                user,
                account.Password[PasswordHashPrefix.Length..],
                password);
 
            if (verification == PasswordVerificationResult.Failed)
            {
                return null;
            }
 
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                account.Password = HashPassword(user, password);
                context.SaveChanges();
            }
 
            return user;
        }
 
        if (!string.Equals(account.Password, password, StringComparison.Ordinal))
        {
            return null;
        }
 
        account.Password = HashPassword(user, password);
        context.SaveChanges();
        return user;
    }
 
    private string HashPassword(UserDto user, string password) =>
        PasswordHashPrefix + passwordHasher.HashPassword(user, password);
 
    private static UserDto ToUserDto(UserAccount account) =>
        new()
        {
            UserId = account.UserId,
            Username = account.Username,
            Password = account.Password,
            Role = account.Role,
            FullName = account.FullName,
            Email = account.Email,
            RegistrationStatus = account.RegistrationStatus,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            Roles = account.UserRoles
                .Select(userRole => userRole.Role.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            UserRoles = account.UserRoles
        };
}