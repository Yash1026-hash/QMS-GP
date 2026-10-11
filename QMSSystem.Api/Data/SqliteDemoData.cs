using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Data;

public static class SqliteDemoData
{
    private const string PasswordHashPrefix = "hash$v1$";

    public static async Task InitializeAsync(
        UserDbContext database,
        IPasswordHasher<UserDto> passwordHasher)
    {
        var roles = new[] { "Admin", "Operator", "Supervisor" };
        foreach (var roleName in roles)
        {
            if (!await database.Roles.AnyAsync(role => role.Name == roleName))
            {
                database.Roles.Add(new Role { Name = roleName });
            }
        }

        await database.SaveChangesAsync();

        var demoUsers = new[]
        {
            new { Username = "admin", Password = "admin123", Role = "Admin", FullName = "QMS Admin", Email = "admin@qms.local" },
            new { Username = "operator", Password = "operator123", Role = "Operator", FullName = "QMS Operator", Email = "operator@qms.local" },
            new { Username = "supervisor", Password = "supervisor123", Role = "Supervisor", FullName = "QMS Supervisor", Email = "supervisor@qms.local" }
        };

        foreach (var demoUser in demoUsers)
        {
            var user = await database.Users
                .Include(account => account.UserRoles)
                .FirstOrDefaultAsync(account => account.Username == demoUser.Username);

            if (user is null)
            {
                var userDto = new UserDto { Username = demoUser.Username };
                user = new UserAccount
                {
                    Username = demoUser.Username,
                    Password = PasswordHashPrefix + passwordHasher.HashPassword(userDto, demoUser.Password),
                    Role = demoUser.Role,
                    FullName = demoUser.FullName,
                    Email = demoUser.Email,
                    RegistrationStatus = "Registered",
                    Department = "Demo",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                database.Users.Add(user);
            }

            var role = await database.Roles.SingleAsync(item => item.Name == demoUser.Role);
            if (!user.UserRoles.Any(userRole => userRole.RoleId == role.RoleId))
            {
                user.UserRoles.Add(new UserRole
                {
                    User = user,
                    Role = role
                });
            }
        }

        await database.SaveChangesAsync();

        if (await database.DeviationRequests.AnyAsync())
        {
            return;
        }

        var operatorUser = await database.Users.SingleAsync(user => user.Username == "operator");
        var document = new DocumentCreation
        {
            DocumentNumber = "DEMO-SOP-001",
            Title = "Demonstration Standard Operating Procedure",
            Department = "Quality",
            DocumentVersion = 1,
            Status = "Approved",
            FileName = "demo-sop.txt",
            ContentType = "text/plain",
            FileData = [],
            CreatedBy = operatorUser.UserId,
            CreationOn = DateTime.UtcNow,
            Comment = "Local presentation sample"
        };
        database.DocumentCreations.Add(document);
        await database.SaveChangesAsync();

        var deviation = new DeviationRequest
        {
            Title = "Demonstration pending deviation",
            Description = "Sample deviation seeded for a local presentation.",
            DocumentId = document.Id,
            Priority = "Medium",
            Status = 0,
            CreatedBy = "QMS Operator",
            CreatedDate = DateTime.UtcNow
        };
        database.DeviationRequests.Add(deviation);
        await database.SaveChangesAsync();

        database.DeviationReportRequests.Add(new DeviationReportRequest
        {
            DeviationId = deviation.Id,
            DocumentId = document.Id,
            Summary = "Demonstration investigation report awaiting supervisor review.",
            CreatedBy = "QMS Operator",
            CreatedDate = DateTime.UtcNow,
            Status = 0
        });
        await database.SaveChangesAsync();
    }
}
