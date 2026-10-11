using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class SelectSopModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SelectSopModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<DocumentCreation> ActiveSops { get; private set; } = new();
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");

            var documents = await client.GetFromJsonAsync<List<DocumentCreation>>(
                "api/Documents");

            ActiveSops = (documents ?? new List<DocumentCreation>())
                .Where(d =>
                    string.Equals(d.Status, "Active",
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(d.Status, "Approved",
                        StringComparison.OrdinalIgnoreCase))
                .Where(d =>
                    string.IsNullOrWhiteSpace(Search)
                    || (d.DocumentNumber?.Contains(
                        Search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (d.Title?.Contains(
                        Search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (d.Department?.Contains(
                        Search, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }
        catch (HttpRequestException)
        {
            ErrorMessage =
                "Could not retrieve documents. Check that the API is running and the API URL is correct.";
        }
        catch (System.Text.Json.JsonException)
        {
            ErrorMessage =
                "The document API returned data that could not be read.";
        }
    }
}