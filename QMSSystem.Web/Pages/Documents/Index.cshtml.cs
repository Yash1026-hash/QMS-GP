using Microsoft.AspNetCore.Mvc;
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

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

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

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var searchTerm = SearchQuery.Trim();
            Documents = Documents
                .Where(document =>
                    document.DocumentNumber.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase) ||
                    document.Title.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase) ||
                    document.Department.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase) ||
                    document.Status.Contains(
                        searchTerm,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            Documents = Documents
                .Where(document => document.Status.Equals(
                    StatusFilter,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}