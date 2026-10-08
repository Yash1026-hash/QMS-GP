using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Account;

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
                new(ClaimTypes.Name, string.IsNullOrWhiteSpace(login.FullName) ? login.Username : login.FullName)
            };
            claims.AddRange((login.Roles.Count > 0 ? login.Roles : [login.Role])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(role => new Claim(ClaimTypes.Role, role)));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            var roles = (login.Roles.Count > 0 ? login.Roles : [login.Role]);
            var isAdmin = roles.Any(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
            var isSupervisor = roles.Any(r => string.Equals(r, "Supervisor", StringComparison.OrdinalIgnoreCase));
            var isOperator = roles.Any(r => string.Equals(r, "Operator", StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl) &&
                    ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Admin/Index");
            }

            if (isOperator)
            {
                if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl) &&
                    !ReturnUrl.StartsWith("/Supervisor", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.Equals("/", StringComparison.Ordinal))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Operator/Index");
            }

            if (isSupervisor)
            {
                if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                    Url.IsLocalUrl(ReturnUrl) &&
                    !ReturnUrl.StartsWith("/Operator", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) &&
                    !ReturnUrl.Equals("/", StringComparison.Ordinal))
                {
                    return Redirect(ReturnUrl);
                }

                return RedirectToPage("/Supervisor/Index");
            }

            if (!string.IsNullOrWhiteSpace(ReturnUrl) &&
                Url.IsLocalUrl(ReturnUrl) &&
                !ReturnUrl.StartsWith("/Account", StringComparison.OrdinalIgnoreCase) &&
                !ReturnUrl.Equals("/", StringComparison.Ordinal))
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