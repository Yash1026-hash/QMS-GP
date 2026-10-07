using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Supervisor;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<SupervisorModule> Modules { get; set; } = new();

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        try
        {
            var result = await client.GetFromJsonAsync<List<SupervisorModule>>(
                "/api/supervisor/modules");

            if (result != null)
            {
                Modules = result;
            }
        }
        catch
        {
            Modules = new List<SupervisorModule>();
        }
    }
}

public class SupervisorModule
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Route { get; set; } = string.Empty;
}