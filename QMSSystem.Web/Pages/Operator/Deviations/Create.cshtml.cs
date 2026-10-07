using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Enums;

namespace QMSSystem.Web.Pages.Deviations;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty(SupportsGet = true)]
    public int DocumentId { get; set; }

    public ActiveSopDto? SelectedSop { get; private set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

  [BindProperty]
public Priority? Priority { get; set; }
    [BindProperty]
    public List<IFormFile> Proofs { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (DocumentId <= 0)
        {
            return RedirectToPage("/Deviations/SelectSop");
        }

        var loaded = await LoadSelectedSopAsync();

        if (!loaded)
        {
            return RedirectToPage("/Deviations/SelectSop");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DocumentId <= 0)
        {
            return RedirectToPage("/Deviations/SelectSop");
        }

        await LoadSelectedSopAsync();

        if (SelectedSop is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The selected SOP could not be found.");

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError(
                nameof(Title),
                "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(
                nameof(Description),
                "Description is required.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }
        if (!Priority.HasValue)
{
    ModelState.AddModelError(
        nameof(Priority),
        "Priority / Severity is required.");
}

        var client = _httpClientFactory.CreateClient("ApiClient");

        using var form = new MultipartFormDataContent();

        form.Add(
            new StringContent(Title),
            "Title");

        form.Add(
            new StringContent(Description),
            "Description");

        form.Add(
            new StringContent(DocumentId.ToString()),
            "DocumentId");

        form.Add(
            new StringContent(Priority.Value.ToString()),
            "Priority");

        foreach (var proof in Proofs)
        {
            if (proof.Length <= 0)
            {
                continue;
            }

            var streamContent =
                new StreamContent(proof.OpenReadStream());

            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(proof.ContentType)
                        ? "application/octet-stream"
                        : proof.ContentType);

            form.Add(
                streamContent,
                "Proofs",
                proof.FileName);
        }

        try
        {
            var response =
                await client.PostAsync(
                    "/api/deviations",
                    form);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToPage("/Deviations/Index");
            }

            var error =
                await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                string.IsNullOrWhiteSpace(error)
                    ? "The deviation could not be created."
                    : error);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "The QMS API could not be reached.");
        }

        return Page();
    }

    private async Task<bool> LoadSelectedSopAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");

        try
        {
            var response =
                await client.GetAsync(
                    $"/api/documents/{DocumentId}");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            SelectedSop =
                await response.Content
                    .ReadFromJsonAsync<ActiveSopDto>();

            return SelectedSop is not null;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}