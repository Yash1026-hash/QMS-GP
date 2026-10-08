using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Documents;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<Document> Documents { get; set; } = [];

    public async Task OnGetAsync()
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        var response = await client.GetAsync("api/Documents");

        if (response.IsSuccessStatusCode)
        {
            Documents = await response.Content
                .ReadFromJsonAsync<List<Document>>() ?? [];
        }
    }
}