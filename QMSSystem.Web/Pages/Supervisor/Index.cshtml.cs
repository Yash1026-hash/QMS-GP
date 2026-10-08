using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Supervisor;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public int PendingDeviations { get; set; } = 3;

    public int PendingChangeRequests { get; set; }

    public int PendingReports { get; set; } = 2;

    public int PendingDocuments { get; set; } = 5;

    public int TotalPending { get; set; }

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        try
        {
            PendingChangeRequests =
                await client.GetFromJsonAsync<int>(
                    "api/supervisor/change-requests/count");
        }
        catch
        {
            PendingChangeRequests = 0;
        }

        TotalPending =
            PendingDeviations +
            PendingChangeRequests +
            PendingReports +
            PendingDocuments;
    }
}