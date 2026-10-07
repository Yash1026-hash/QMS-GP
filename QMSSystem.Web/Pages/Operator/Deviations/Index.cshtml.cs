using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Enums;

namespace QMSSystem.Web.Pages.Deviations;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<DeviationListDto> Deviations { get; private set; } = new();

    public int TotalCount { get; private set; }

    public int TotalPages { get; private set; }

    public int PageSize { get; private set; } = 10;

    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public Priority? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public DeviationStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        var client = _httpClientFactory.CreateClient("ApiClient");

        var parameters = new List<string>
        {
            $"pageNumber={PageNumber}",
            $"pageSize={PageSize}"
        };

        if (!string.IsNullOrWhiteSpace(Search))
        {
            parameters.Add(
                $"search={Uri.EscapeDataString(Search.Trim())}");
        }

        if (Priority.HasValue)
        {
            parameters.Add(
                $"priority={Priority.Value}");
        }

        if (Status.HasValue)
        {
            parameters.Add(
                $"status={Status.Value}");
        }

        var url = "/api/deviations?" +
                  string.Join("&", parameters);

        try
        {
            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage =
                    "Unable to load deviations.";

                return;
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<DeviationListResponseDto>();

            if (result == null)
            {
                ErrorMessage =
                    "No deviation data was returned.";

                return;
            }

            Deviations = result.Items;

            TotalCount = result.TotalCount;

            TotalPages = result.TotalPages;

            PageNumber = result.PageNumber;
        }
        catch
        {
            ErrorMessage =
                "Unable to connect to the deviation service.";
        }
    }
}