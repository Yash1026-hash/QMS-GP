using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class IndexModel(
    IHttpClientFactory httpClientFactory,
    ILogger<IndexModel> logger) : PageModel
{
    public List<DeviationRequestDto> Deviations { get; private set; } = new();

    public int TotalCount { get; private set; }

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

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        try
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            using var response = await client.GetAsync("/api/Deviation");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Your session has expired. Sign in again to view deviations.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage = "You are not authorized to view deviations.";
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "API returned HTTP {StatusCode} while loading deviations",
                    (int)response.StatusCode);
                ErrorMessage = $"The API could not load deviations (HTTP {(int)response.StatusCode}).";
                return;
            }

            var allDeviations =
                await response.Content.ReadFromJsonAsync<List<DeviationRequestDto>>();

            if (allDeviations is null)
            {
                logger.LogError("API returned an empty response body while loading deviations");
                ErrorMessage = "The API returned no deviation list.";
                return;
            }

            IEnumerable<DeviationRequestDto> filtered = allDeviations;
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var search = Search.Trim();
                filtered = filtered.Where(deviation =>
                    deviation.Title.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || deviation.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || deviation.CreatedBy.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || deviation.DocumentId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(Priority))
            {
                filtered = filtered.Where(deviation =>
                    string.Equals(deviation.Priority, Priority, StringComparison.OrdinalIgnoreCase));
            }

            if (Status.HasValue)
            {
                filtered = filtered.Where(deviation => deviation.Status == Status.Value);
            }

            var filteredDeviations = filtered
                .OrderByDescending(deviation => deviation.CreatedDate)
                .ToList();
            TotalCount = filteredDeviations.Count;

            var totalPages = TotalPages;
            if (totalPages > 0 && PageNumber > totalPages)
            {
                PageNumber = totalPages;
            }

            Deviations = filteredDeviations
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not connect to the API while loading deviations");
            ErrorMessage = "Could not connect to the API while loading deviations.";
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "API returned invalid deviation data");
            ErrorMessage = "The API returned deviation data in an unexpected format.";
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

    public string GetStatusClass(int status)
    {
        return status switch
        {
            0 => "status-pending",
            1 => "status-active",
            2 => "status-inactive",
            _ => "status-inactive"
        };
    }

    public string GetPriorityText(string? priority)
    {
        return string.IsNullOrWhiteSpace(priority)
            ? "-"
            : priority;
    }
}