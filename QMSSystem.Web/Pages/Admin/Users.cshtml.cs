using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin;

public class UsersModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UsersModel> _logger;

    public UsersModel(IHttpClientFactory httpClientFactory, ILogger<UsersModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DepartmentFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<AdminUserItemDto> Users { get; set; } = [];
    public List<AdminRoleItemDto> AvailableRoles { get; set; } = [];

    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingUsers { get; set; }

    [TempData]
    public string? SuccessMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync(int userId, string newRole)
    {
        if (userId <= 0 || string.IsNullOrWhiteSpace(newRole))
        {
            ErrorMessage = "Invalid user or role selection.";
            return RedirectToPage();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var request = new ChangeUserRoleRequest { Role = newRole };
            var response = await client.PostAsJsonAsync($"api/admin/users/{userId}/role", request);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = $"Role updated to '{newRole}' successfully for User #{userId}.";
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Failed to update role: {err}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to change role for user {UserId}", userId);
            ErrorMessage = "An error occurred while changing user role.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int userId, bool isActive)
    {
        if (userId <= 0)
        {
            ErrorMessage = "Invalid user specified.";
            return RedirectToPage();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var request = new UpdateUserStatusRequest { IsActive = isActive };
            var response = await client.PutAsJsonAsync($"api/admin/users/{userId}/status", request);

            if (response.IsSuccessStatusCode)
            {
                SuccessMessage = $"User #{userId} status updated to {(isActive ? "Active" : "Inactive")}.";
            }
            else
            {
                ErrorMessage = "Failed to update user status.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle status for user {UserId}", userId);
            ErrorMessage = "An error occurred while updating user status.";
        }

        return RedirectToPage();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");

            AvailableRoles = await client.GetFromJsonAsync<List<AdminRoleItemDto>>("api/admin/roles") ?? [];
            var allUsers = await client.GetFromJsonAsync<List<AdminUserItemDto>>("api/admin/users") ?? [];

            TotalUsers = allUsers.Count;
            ActiveUsers = allUsers.Count(u => u.IsActive);
            PendingUsers = allUsers.Count(u => u.RegistrationStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase));

            var query = allUsers.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(RoleFilter))
            {
                query = query.Where(u => u.Role.Equals(RoleFilter, StringComparison.OrdinalIgnoreCase) ||
                                         u.Roles.Any(r => r.Equals(RoleFilter, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(DepartmentFilter))
            {
                query = query.Where(u => u.Department.Equals(DepartmentFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(u =>
                    u.Username.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    u.FullName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    u.Department.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            Users = query.OrderBy(u => u.UserId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load users from API");
            ErrorMessage = "Unable to load users data from API.";
        }
    }
}

