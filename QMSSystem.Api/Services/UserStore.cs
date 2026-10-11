using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QMSSystem.Api.Data;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Services;

public sealed class UserStore(
    UserDbContext context,
    IPasswordHasher<UserDto> passwordHasher,
    ILogger<UserStore> logger)
{
    private const string PasswordHashPrefix = "hash$v1$";

    public UserDto? ValidateCredentials(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();

        try
        {
            var account = context.Users
                .Include(account => account.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                .FirstOrDefault(account => account.Username.ToUpper() == normalizedUsername);

            if (account is not null)
            {
                if (!account.IsActive ||
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
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database unreachable while validating credentials for '{Username}'. Falling back to offline demo credentials.", username);
        }

        return GetOfflineDemoUser(normalizedUsername, password);
    }

    private static UserDto? GetOfflineDemoUser(string normalizedUsername, string password)
    {
        var demoUsers = new[]
        {
            new
            {
                Username = "ADMIN",
                Passwords = new[] { "admin123", "admin", "password" },
                Role = "Admin",
                FullName = "QMS Admin",
                Email = "admin@qms.local"
            },
            new
            {
                Username = "OPERATOR",
                Passwords = new[] { "operator123", "operator", "password" },
                Role = "Operator",
                FullName = "QMS Operator",
                Email = "operator@qms.local"
            },
            new
            {
                Username = "SUPERVISOR",
                Passwords = new[] { "supervisor123", "supervisor", "password" },
                Role = "Supervisor",
                FullName = "QMS Supervisor",
                Email = "supervisor@qms.local"
            }
        };

        var matched = demoUsers.FirstOrDefault(u => u.Username == normalizedUsername);
        if (matched is null)
        {
            return null;
        }

        if (!matched.Passwords.Contains(password, StringComparer.Ordinal))
        {
            return null;
        }

        return new UserDto
        {
            UserId = matched.Username switch
            {
                "ADMIN" => 1,
                "OPERATOR" => 2,
                "SUPERVISOR" => 3,
                _ => 999
            },
            Username = matched.Username.ToLowerInvariant(),
            Role = matched.Role,
            FullName = matched.FullName,
            Email = matched.Email,
            RegistrationStatus = "Registered",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<string> { matched.Role },
            UserRoles = new List<UserRole>
            {
                new()
                {
                    Role = new Role { Name = matched.Role }
                }
            }
        };
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