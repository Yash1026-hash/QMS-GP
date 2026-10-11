using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin;

public class ReportsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReportsModel> _logger;

    public ReportsModel(IHttpClientFactory httpClientFactory, ILogger<ReportsModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int? DecisionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public List<DeviationReportRequest> Reports { get; set; } = [];
    public int TotalReports { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int PendingCount { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var allReports = await client.GetFromJsonAsync<List<DeviationReportRequest>>("api/admin/reports") ?? [];

            TotalReports = allReports.Count;
            ApprovedCount = allReports.Count(r => r.Decision == 1);
            RejectedCount = allReports.Count(r => r.Decision == 2);
            PendingCount = allReports.Count(r => !r.Decision.HasValue);

            var query = allReports.AsEnumerable();

            if (DecisionFilter.HasValue)
            {
                if (DecisionFilter.Value == -1)
                {
                    query = query.Where(r => !r.Decision.HasValue);
                }
                else
                {
                    query = query.Where(r => r.Decision == DecisionFilter.Value);
                }
            }

            if (StatusFilter.HasValue)
            {
                query = query.Where(r => r.Status == StatusFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                query = query.Where(r =>
                    r.Summary.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.CreatedBy.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    r.DeviationId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (r.DecisionComments != null && r.DecisionComments.Contains(term, StringComparison.OrdinalIgnoreCase)));
            }

            Reports = query.OrderByDescending(r => r.CreatedDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load reports");
            ErrorMessage = "Failed to load deviation reports from API.";
        }
    }
}

