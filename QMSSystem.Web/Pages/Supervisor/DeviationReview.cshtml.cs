
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Supervisor;

public class DeviationReviewModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    private const string ApiBaseUrl = "http://localhost:5070";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DeviationReviewModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty(SupportsGet = true)]
    public int? SearchId { get; set; }

    public DeviationRequestDto? Deviation { get; set; }

    [BindProperty]
    public DeviationApproval Approval { get; set; } = new();

    public string? Message { get; set; }


    public async Task OnGetAsync()
    {
        if (!SearchId.HasValue || SearchId.Value <= 0)
        {
            return;
        }

        await LoadDeviationAsync();

        if (Deviation == null && Message == null)
        {
            Message = $"Deviation with ID {SearchId.Value} was not found.";
        }
    }


    public async Task<IActionResult> OnPostApproveAsync()
    {
        Approval.Decision = 1;

        return await SubmitDecisionAsync();
    }


    public async Task<IActionResult> OnPostRejectAsync()
    {
        Approval.Decision = 2;

        if (string.IsNullOrWhiteSpace(Approval.DecisionComments))
        {
            SearchId = Approval.DeviationRequestId;

            Message = "Comments are required when rejecting a deviation.";

            await LoadDeviationAsync();

            return Page();
        }

        return await SubmitDecisionAsync();
    }


    private async Task<IActionResult> SubmitDecisionAsync()
    {
        int deviationId = Approval.DeviationRequestId;

        if (deviationId <= 0)
        {
            Message = "Invalid deviation ID.";
            return Page();
        }

        var client = CreateApiClient();

        var requestBody = new
        {
            decision = Approval.Decision,
            decisionComments = Approval.DecisionComments
        };

        string json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await client.PostAsync(
            $"{ApiBaseUrl}/api/deviation-review/{deviationId}",
            content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage(
                new { SearchId = deviationId });
        }

        SearchId = deviationId;

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            Message = await response.Content.ReadAsStringAsync();
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Message = "Deviation was not found or has already been processed.";
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Message = "You are not authenticated. Please sign in again.";
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Message = "You are not authorized to review deviations.";
        }
        else
        {
            Message = "Unable to submit the decision. Please try again.";
        }

        await LoadDeviationAsync();

        return Page();
    }


    private async Task LoadDeviationAsync()
    {
        Deviation = null;

        if (!SearchId.HasValue || SearchId.Value <= 0)
        {
            Message = "Enter a valid deviation ID.";
            return;
        }

        var client = CreateApiClient();

        using var response = await client.GetAsync(
            $"{ApiBaseUrl}/api/deviation-review/{SearchId.Value}");

        if (response.IsSuccessStatusCode)
        {
            string json = await response.Content.ReadAsStringAsync();

            Deviation = JsonSerializer.Deserialize<DeviationRequestDto>(
                json,
                JsonOptions);

            if (Deviation != null)
            {
                Approval.DeviationRequestId = Deviation.Id;
            }
            else
            {
                Message = "The API returned no deviation data.";
            }

            return;
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            Message = $"Deviation with ID {SearchId.Value} was not found.";
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Message = "You are not authenticated. Please sign in again.";
        }
        else if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            Message = "You are not authorized to review deviations.";
        }
        else
        {
            Message = "Unable to load the deviation. Please try again.";
        }
    }


    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        if (Request.Headers.TryGetValue("Cookie", out var cookie))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "Cookie",
                cookie.ToString());
        }

        return client;
    }
}
