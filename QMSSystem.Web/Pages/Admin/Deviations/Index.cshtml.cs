using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin.Deviations;

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
    public int? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? PriorityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DecisionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int ActiveCount { get; set; }
    public int InactiveCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int? LatestDeviationId { get; set; }

    public List<DeviationRequestDto> Deviations { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            List<DeviationRequestDto>? allDeviations = null;

            try
            {
                allDeviations = await client.GetFromJsonAsync<List<DeviationRequestDto>>("api/admin/deviations");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "api/admin/deviations failed, trying api/Deviation fallback");
                allDeviations = await client.GetFromJsonAsync<List<DeviationRequestDto>>("api/Deviation");
            }

            allDeviations ??= [];

            TotalCount = allDeviations.Count;
            LatestDeviationId = allDeviations.Count > 0 ? allDeviations.Max(d => d.Id) : null;
            PendingCount = allDeviations.Count(d => d.Status == 0);
            ActiveCount = allDeviations.Count(d => d.Status == 1);
            InactiveCount = allDeviations.Count(d => d.Status == 2);
            ApprovedCount = allDeviations.Count(d => d.Decision == 1);
            RejectedCount = allDeviations.Count(d => d.Decision == 2);

            var query = allDeviations.AsEnumerable();

            if (StatusFilter.HasValue)
            {
                query = query.Where(d => d.Status == StatusFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(PriorityFilter))
            {
                query = query.Where(d => d.Priority != null && d.Priority.Equals(PriorityFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (DecisionFilter.HasValue)
            {
                if (DecisionFilter.Value == -1)
                {
                    query = query.Where(d => !d.Decision.HasValue);
                }
                else
                {
                    query = query.Where(d => d.Decision == DecisionFilter.Value);
                }
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var term = SearchQuery.Trim();
                var cleanId = term.TrimStart('#').Replace("DEV-", "", StringComparison.OrdinalIgnoreCase).Replace("DEV", "", StringComparison.OrdinalIgnoreCase).Trim();

                query = query.Where(d =>
                    d.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(cleanId) && d.Id.ToString().Contains(cleanId, StringComparison.OrdinalIgnoreCase)) ||
                    d.DocumentId.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(cleanId) && d.DocumentId.ToString().Contains(cleanId, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Title != null && d.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Description != null && d.Description.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (d.CreatedBy != null && d.CreatedBy.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (d.Priority != null && d.Priority.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (d.DecisionBy != null && d.DecisionBy.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (d.DecisionComments != null && d.DecisionComments.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    (term.Equals("pending", StringComparison.OrdinalIgnoreCase) && (d.Status == 0 || !d.Decision.HasValue)) ||
                    (term.Equals("active", StringComparison.OrdinalIgnoreCase) && d.Status == 1) ||
                    (term.Equals("inactive", StringComparison.OrdinalIgnoreCase) && d.Status == 2) ||
                    (term.Equals("approved", StringComparison.OrdinalIgnoreCase) && d.Decision == 1) ||
                    (term.Equals("rejected", StringComparison.OrdinalIgnoreCase) && d.Decision == 2));
            }

            Deviations = query
                .OrderByDescending(d => d.CreatedDate)
                .ThenByDescending(d => d.Id)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deviations");
            ErrorMessage = $"Unable to load deviations: {ex.Message}";
        }
    }
}
