using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Supervisor;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public int PendingDeviations { get; set; }

    public int PendingChangeRequests { get; set; }

    public int PendingReports { get; set; }

    public int PendingDocuments { get; set; }

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

        // Get pending deviation reports (Status = 0)
        try
        {
            PendingReports =
                await client.GetFromJsonAsync<int>(
                    "api/supervisor/deviation-reports/count");
        }
        catch
        {
            PendingReports = 0;
        }

        // Get pending documents (Status = 0)
        try
        {
            PendingDocuments =
                await client.GetFromJsonAsync<int>(
                    "api/supervisor/documents/count");
        }
        catch
        {
            PendingDocuments = 0;
        }

        // Calculate total pending items
        TotalPending =
            PendingDeviations +
            PendingChangeRequests +
            PendingReports +
            PendingDocuments;
    }
}