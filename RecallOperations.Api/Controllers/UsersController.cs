using Microsoft.AspNetCore.Mvc;
using RecallOperations.Api.Services;
using VehicleRecall.Shared.Models;

namespace RecallOperations.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(
    UserStore userStore,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
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
        return user is null
            ? Unauthorized(new { message = "Invalid username or password." })
            : Ok(jwtTokenService.Create(user));
    }
}
