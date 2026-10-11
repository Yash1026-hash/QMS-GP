using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public List<DeviationRequestDto> Deviations { get; private set; } = new();

    public int TotalCount => Deviations.Count;

    public int PageSize { get; private set; } = 10;

    public int TotalPages =>
        TotalCount == 0
            ? 0
            : (int)Math.Ceiling((double)TotalCount / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var response = await client.GetAsync("api/Deviation");
            if (response.IsSuccessStatusCode)
            {
                var allDeviations = await response.Content.ReadFromJsonAsync<List<DeviationRequestDto>>() ?? [];

                var query = allDeviations.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(Search))
                {
                    var term = Search.Trim();
                    query = query.Where(d =>
                        d.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        d.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        d.Id.ToString().Contains(term) ||
                        d.DocumentId.ToString().Contains(term));
                }

                if (!string.IsNullOrWhiteSpace(Priority))
                {
                    query = query.Where(d => d.Priority.Equals(Priority, StringComparison.OrdinalIgnoreCase));
                }

                if (Status.HasValue)
                {
                    query = query.Where(d => d.Status == Status.Value);
                }

                Deviations = query.OrderByDescending(d => d.CreatedDate).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deviations from API.");
        }
    }

    public string GetStatusText(int status)
    {
        return status switch
        {
            0 => "Pending",
            1 => "Active",
            2 => "Inactive",
            _ => "Unknown"
        };
    }

    public string GetPriorityText(string? priority)
    {
        return string.IsNullOrWhiteSpace(priority)
            ? "-"
            : priority;
    }
}