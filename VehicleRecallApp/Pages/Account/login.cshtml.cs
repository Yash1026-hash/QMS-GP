using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VehicleRecall.Shared.Models;

namespace VehicleRecallApp.Pages.Account;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return Page();
        }

        var client = HttpContext.RequestServices
            .GetRequiredService<IHttpClientFactory>()
            .CreateClient("ApiClient");

        var loginRequest = new LoginRequest
        {
            Username = Username.Trim(),
            Password = Password
        };

        try
        {
            var response = await client.PostAsJsonAsync("/api/users/login", loginRequest);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (login is null)
            {
                ErrorMessage = "Login failed.";
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(login.FullName) ? login.Username : login.FullName),
                new("access_token", login.AccessToken)
            };
            claims.AddRange((login.Roles.Count > 0 ? login.Roles : [login.Role])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { ExpiresUtc = login.ExpiresAt });

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
            {
                return Redirect(ReturnUrl);
            }

            return RedirectToPage("/Index");
        }
        catch
        {
            ErrorMessage = "Could not connect to the API server.";
            return Page();
        }
    }
    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToPage("/Index");
    }
}