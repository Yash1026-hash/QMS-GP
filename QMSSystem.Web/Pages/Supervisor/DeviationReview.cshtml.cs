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

        var client = CreateApiClient();

        var response = await client.GetAsync(
            $"{ApiBaseUrl}/api/deviation-review/{SearchId.Value}");

        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();

            Deviation = JsonSerializer.Deserialize<DeviationRequestDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (Deviation != null)
            {
                Approval.DeviationRequestId = Deviation.Id;
            }

            return;
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            Message = $"Deviation with ID {SearchId.Value} was not found.";
            return;
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            Message = "You are not authenticated.";
            return;
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Forbidden)
        {
            Message = "You are not authorized to review deviations.";
            return;
        }

        Message = "Unable to load the deviation.";
    }


    public async Task<IActionResult> OnPostApproveAsync()
    {
        Approval.Decision = 1;

        return await SubmitDecisionAsync();
    }


    public async Task<IActionResult> OnPostRejectAsync()
    {
        Approval.Decision = 2;

        if (string.IsNullOrWhiteSpace(
            Approval.DecisionComments))
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
        var deviationId = Approval.DeviationRequestId;

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

        var json = JsonSerializer.Serialize(requestBody);

        using var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync(
            $"{ApiBaseUrl}/api/deviation-review/{deviationId}",
            content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToPage(
                new
                {
                    SearchId = deviationId
                });
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            Message =
                "Deviation was not found or has already been processed.";

            SearchId = deviationId;

            await LoadDeviationAsync();

            return Page();
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.BadRequest)
        {
            var error =
                await response.Content.ReadAsStringAsync();

            Message = error;

            SearchId = deviationId;

            await LoadDeviationAsync();

            return Page();
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            Message = "You are not authenticated.";

            return Page();
        }

        if (response.StatusCode ==
            System.Net.HttpStatusCode.Forbidden)
        {
            Message = "You are not authorized to review deviations.";

            return Page();
        }

        Message = "Unable to submit the decision.";

        return Page();
    }


    private async Task LoadDeviationAsync()
    {
        if (SearchId == null || SearchId <= 0)
        {
            return;
        }

        var client = CreateApiClient();

        var response = await client.GetAsync(
            $"{ApiBaseUrl}/api/deviation-review/{SearchId.Value}");

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var json = await response.Content.ReadAsStringAsync();

        Deviation = JsonSerializer.Deserialize<DeviationRequestDto>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (Deviation != null)
        {
            Approval.DeviationRequestId = Deviation.Id;
        }
    }


    private HttpClient CreateApiClient()
    {
        var client = _httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(
                "application/json"));

        if (Request.Headers.TryGetValue(
                "Cookie",
                out var cookie))
        {
            client.DefaultRequestHeaders.Add(
                "Cookie",
                cookie.ToString());
        }

        return client;
    }
}