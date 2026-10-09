using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;
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
    public Dictionary<string, int> DepartmentDocCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> DepartmentUserCounts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public int TotalRoles { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalControlledDocuments { get; set; }
    public int TotalChangeRequests { get; set; }
    public int TotalDeviations { get; set; }
    public int TotalInvestigationReports { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");

            Roles = await client.GetFromJsonAsync<List<AdminRoleItemDto>>("api/admin/roles") ?? [];
            TotalRoles = Roles.Count;

            var users = await client.GetFromJsonAsync<List<AdminUserItemDto>>("api/admin/users") ?? [];
            foreach (var r in Roles)
            {
                RoleUserCounts[r.Name] = users.Count(u => u.Role.Equals(r.Name, StringComparison.OrdinalIgnoreCase));
            }

            var docs = await client.GetFromJsonAsync<List<Document>>("api/admin/documents") ?? [];
            TotalControlledDocuments = docs.Count;

            var stats = await client.GetFromJsonAsync<AdminDashboardStatsDto>("api/admin/stats");
            if (stats != null)
            {
                TotalChangeRequests = stats.TotalChangeRequests;
                TotalDeviations = stats.TotalDeviations;
            }

            var reports = await client.GetFromJsonAsync<List<DeviationReportRequest>>("api/admin/reports") ?? [];
            TotalInvestigationReports = reports.Count;

            // Merge unique departments from both documents and users
            var deptsFromUsers = users
                .Select(u => u.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d));

            var deptsFromDocs = docs
                .Select(d => d.Department)
                .Where(d => !string.IsNullOrWhiteSpace(d));

            Departments = deptsFromUsers
                .Concat(deptsFromDocs)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(d => d)
                .ToList();

            TotalDepartments = Departments.Count;

            foreach (var dept in Departments)
            {
                DepartmentDocCounts[dept] = docs.Count(d => d.Department.Equals(dept, StringComparison.OrdinalIgnoreCase));
                DepartmentUserCounts[dept] = users.Count(u => u.Department.Equals(dept, StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load master data");
            ErrorMessage = "Failed to load master data from API.";
        }
    }
}
