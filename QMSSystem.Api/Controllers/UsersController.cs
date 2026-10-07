using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QMSSystem.Api.Services;
using QMSSystem.Shared.Models;

namespace QMSSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(
    UserStore userStore) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var user = userStore.ValidateCredentials(request.Username, request.Password);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        var roles = user.UserRoles
            .Select(userRole => userRole.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (roles.Count == 0)
        {
            roles.Add(user.Role);
        }

        return Ok(new LoginResponse(
            user.UserId,
            user.Username,
            user.Role,
            user.FullName,
            user.Email)
        {
            Roles = roles
        });
    }
}
