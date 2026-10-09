using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin.ChangeRequests;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ChangeTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int PendingCount { get; set; }
    public int InactiveCount { get; set; }

    public List<OperatorChangeRequest> ChangeRequests { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var allRequests = await client.GetFromJsonAsync<List<OperatorChangeRequest>>("api/admin/change-requests") ?? [];

            TotalCount = allRequests.Count;
            ActiveCount = allRequests.Count(c => c.Status.Equals("active", StringComparison.OrdinalIgnoreCase) || c.Status.Equals("approved", StringComparison.OrdinalIgnoreCase));
            PendingCount = allRequests.Count(c => c.Status.Equals("pending", StringComparison.OrdinalIgnoreCase));
            InactiveCount = allRequests.Count(c => c.Status.Equals("inactive", StringComparison.OrdinalIgnoreCase) || c.Status.Equals("rejected", StringComparison.OrdinalIgnoreCase));

            var query = allRequests.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(c => c.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(ChangeTypeFilter))
            {
                query = query.Where(c => c.ChangeType.Equals(ChangeTypeFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(c =>
                    c.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.ChangeType.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    c.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            ChangeRequests = query.OrderByDescending(c => c.RequestedDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load change requests");
            ErrorMessage = "Failed to load change requests from API.";
        }
    }
}
