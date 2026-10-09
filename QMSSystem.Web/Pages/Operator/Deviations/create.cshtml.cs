
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

[AllowAnonymous]
public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public DeviationRequestDto Deviation { get; set; } = new();

    public string? Message { get; private set; }

    public bool IsFormValid { get; private set; }

    public IActionResult OnGet(int documentId = 0)
    {
        Deviation = new DeviationRequestDto
        {
            DocumentId = documentId,
            Status = 0,
            CreatedBy = User.Identity?.Name ?? string.Empty,
            CreatedDate = DateTime.UtcNow,
            Decision = null,
            DecisionBy = null,
            DecisionOn = null,
            DecisionComments = null
        };

        return Page();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        // Validate fields entered by the operator.
        if (Deviation.DocumentId <= 0)
        {
            ModelState.AddModelError(
                "Deviation.DocumentId",
                "Select an active SOP before creating a deviation.");
        }

        if (string.IsNullOrWhiteSpace(Deviation.Title))
        {
            ModelState.AddModelError(
                "Deviation.Title",
                "Enter a deviation title.");
        }

        if (string.IsNullOrWhiteSpace(Deviation.Description))
        {
            ModelState.AddModelError(
                "Deviation.Description",
                "Describe the deviation.");
        }

        if (Deviation.Priority is not ("Minor" or "Major" or "Critical"))
        {
            ModelState.AddModelError(
                "Deviation.Priority",
                "Select a valid priority.");
        }

        // These values are system-managed, not operator inputs.
        ModelState.Remove("Deviation.DecisionBy");
        ModelState.Remove("Deviation.DecisionComments");
        ModelState.Remove("Deviation.Decision");
        ModelState.Remove("Deviation.DecisionOn");
        ModelState.Remove("Deviation.Status");
        ModelState.Remove("Deviation.CreatedBy");
        ModelState.Remove("Deviation.CreatedDate");
        ModelState.Remove("Deviation.Id");

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e =>
                    $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            Message = "Please correct these errors: " +
                      string.Join(" | ", errors);

            IsFormValid = false;
            return Page();
        }

        // Set system-controlled values on the server.
        Deviation.Id = 0;
        Deviation.Status = 0;
        Deviation.CreatedBy = User.Identity?.Name ?? string.Empty;
        Deviation.CreatedDate = DateTime.UtcNow;
        Deviation.Decision = null;
        Deviation.DecisionBy = null;
        Deviation.DecisionOn = null;
        Deviation.DecisionComments = null;

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");

            var response = await client.PostAsJsonAsync(
                "api/Deviation", Deviation);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();

                Message = $"The deviation could not be saved. API response: {apiError}";
                IsFormValid = false;

                return Page();
            }

            var savedDeviation =
                await response.Content.ReadFromJsonAsync<DeviationRequestDto>();

            if (savedDeviation == null || savedDeviation.Id <= 0)
            {
                Message = "The API did not return a valid saved deviation ID.";
                IsFormValid = false;
                return Page();
            }

            IsFormValid = true;

            return RedirectToPage(
                "/Operator/Deviations/Details",
                new { id = savedDeviation.Id });
        }
        catch (HttpRequestException)
        {
            Message = "Could not connect to the QMS API. Check that the API is running.";
            IsFormValid = false;
            return Page();
        }
        catch (JsonException)
        {
            Message = "The API returned an invalid response. The save could not be confirmed.";
            IsFormValid = false;
            return Page();
        }
    }
}
