using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Supervisor;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public int PendingDeviations { get; set; }

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
        var client = _httpClientFactory.CreateClient("QMSApi");

        // Get pending deviations
        try
        {
            PendingDeviations =
                await client.GetFromJsonAsync<int>(
                    "api/supervisor/deviations/count");
        }
        catch
        {
            PendingDeviations = 0;
        }


        // Get pending change requests
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


        // Calculate total
        TotalPending =
            PendingDeviations +
            PendingChangeRequests +
            PendingReports +
            PendingDocuments;
    }
}