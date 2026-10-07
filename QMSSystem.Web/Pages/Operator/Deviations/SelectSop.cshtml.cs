using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Deviations;

public class SelectSopModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public SelectSopModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public List<ActiveSopDto> ActiveSops { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty]
    public int? SelectedSopId { get; set; }

    public async Task OnGetAsync()
    {
        await LoadActiveSopsAsync();
    }

    public async Task<IActionResult> OnPostContinueAsync()
    {
        if (!SelectedSopId.HasValue)
        {
            await LoadActiveSopsAsync();

            ModelState.AddModelError(
                string.Empty,
                "Select an SOP before you continue.");

            return Page();
        }

        return RedirectToPage(
            "/Deviations/Create",
            new { documentId = SelectedSopId.Value });
    }

    private async Task LoadActiveSopsAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        try
        {
            var response =
                await client.GetAsync("/api/documents/active");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The active SOP list could not be loaded.");

                return;
            }

            var sops =
                await response.Content
                    .ReadFromJsonAsync<List<ActiveSopDto>>();

            ActiveSops = sops ?? new List<ActiveSopDto>();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var searchTerm = Search.Trim();

                ActiveSops = ActiveSops
                    .Where(s =>
                        s.DocumentNumber.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        s.Title.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The QMS API could not be reached.");
        }
    }
}