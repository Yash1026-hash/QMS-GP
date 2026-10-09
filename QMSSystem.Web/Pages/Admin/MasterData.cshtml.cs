using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin;

public class MasterDataModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MasterDataModel> _logger;

    public MasterDataModel(IHttpClientFactory httpClientFactory, ILogger<MasterDataModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public List<AdminRoleItemDto> Roles { get; set; } = [];
    public Dictionary<string, int> RoleUserCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Departments { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            Roles = await client.GetFromJsonAsync<List<AdminRoleItemDto>>("api/admin/roles") ?? [];

            var users = await client.GetFromJsonAsync<List<AdminUserItemDto>>("api/admin/users") ?? [];
            foreach (var r in Roles)
            {
                RoleUserCounts[r.Name] = users.Count(u => u.Role.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
            }

            Departments = users
                .Select(u => u.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load master data");
            ErrorMessage = "Failed to load master data from API.";
        }
    }
}

