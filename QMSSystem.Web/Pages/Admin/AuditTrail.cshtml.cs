using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin;

public class AuditTrailModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuditTrailModel> _logger;

    public AuditTrailModel(IHttpClientFactory httpClientFactory, ILogger<AuditTrailModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? DepartmentFilter { get; set; }

    public List<DocumentHistory> AuditRecords { get; set; } = [];
    public int TotalRecords { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var allRecords = await client.GetFromJsonAsync<List<DocumentHistory>>("api/admin/audit-trail") ?? [];

            TotalRecords = allRecords.Count;
            var query = allRecords.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(DepartmentFilter))
            {
                query = query.Where(h => h.Department.Equals(DepartmentFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(h =>
                    h.DocumentNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    h.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    h.Department.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (h.Comment != null && h.Comment.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            AuditRecords = query.OrderByDescending(h => h.ArchivedOn).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load audit trail");
            ErrorMessage = "Failed to load audit trail records from API.";
        }
    }
}

