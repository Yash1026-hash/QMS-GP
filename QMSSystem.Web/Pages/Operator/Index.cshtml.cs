
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Operator;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public int DocumentsCount { get; private set; }

    public int DeviationsCount { get; private set; }

    public int DeviationReportsCount { get; private set; }

    public int ChangeRequestsCount { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var counts = await client.GetFromJsonAsync<DashboardCounts>(
                "api/operator/counts");

            if (counts is not null)
            {
                DocumentsCount = counts.Documents;
                DeviationsCount = counts.Deviations;
                DeviationReportsCount = counts.DeviationReports;
                ChangeRequestsCount = counts.ChangeRequests;
            }
        }
        catch (HttpRequestException)
        {
            // The API could not be reached.
        }
        catch (JsonException)
        {
            // The API response could not be read.
        }
    }

    public class DashboardCounts
    {
        public int Documents { get; set; }

        public int Deviations { get; set; }

        public int DeviationReports { get; set; }

        public int ChangeRequests { get; set; }
    }
}
