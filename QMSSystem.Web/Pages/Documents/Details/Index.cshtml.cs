using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Documents;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public DocumentCreation? Document { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        var response = await client.GetAsync($"api/Documents/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        Document = await response.Content
            .ReadFromJsonAsync<DocumentCreation>();

        if (Document == null)
        {
            return NotFound();
        }

        return Page();
    }
}