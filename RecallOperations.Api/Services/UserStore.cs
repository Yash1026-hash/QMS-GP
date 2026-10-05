using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecallOperations.Api.Data;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Services;

public sealed class UserStore(
    UserDbContext context,
    IPasswordHasher<UserAccount> passwordHasher)
{
    private const string PasswordHashPrefix = "hash$v1$";

    public UserAccount? ValidateCredentials(string username, string password)
    {
        var normalizedUsername = username.Trim().ToUpperInvariant();
        var user = context.Users
            .Include(account => account.UserRoles)
                .ThenInclude(userRole => userRole.Role)
            .FirstOrDefault(account => account.Username.ToUpper() == normalizedUsername);

        if (user is null ||
            !user.IsActive ||
            !string.Equals(user.RegistrationStatus, "Registered", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (user.Password.StartsWith(PasswordHashPrefix, StringComparison.Ordinal))
        {
            var verification = passwordHasher.VerifyHashedPassword(
                user,
                user.Password[PasswordHashPrefix.Length..],
                password);

            if (verification == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = HashPassword(user, password);
                context.SaveChanges();
            }

            return user;
        }

        if (!string.Equals(user.Password, password, StringComparison.Ordinal))
        {
            return null;
        }

        user.Password = HashPassword(user, password);
        context.SaveChanges();
        return user;
    }

    private string HashPassword(UserAccount user, string password) =>
        PasswordHashPrefix + passwordHasher.HashPassword(user, password);
}
