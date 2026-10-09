
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Supervisor;

public class DeviationReviewReportModel : PageModel
{
    // API URL from your current project configuration.
    private const string ApiBaseUrl = "http://localhost:5070";

    private readonly ILogger<DeviationReviewReportModel> _logger;

    public DeviationReviewReportModel(
        ILogger<DeviationReviewReportModel> logger)
    {
        _logger = logger;
    }

    [BindProperty]
    public DeviationReportDto Report { get; set; } = new();

    [BindProperty]
    public int? Decision { get; set; }

    [BindProperty]
    public string DecisionComments { get; set; } = string.Empty;

    // Search using Deviation ID.
    [BindProperty(SupportsGet = true)]
    public int Search { get; set; }

    public string SearchMessage { get; private set; } = string.Empty;

    public string ErrorMessage { get; private set; } = string.Empty;

    public bool IsSubmitted { get; set; }

    public string DecisionMessage { get; set; } = string.Empty;

    public bool IsApproved => Report.Decision == 1;

    public bool IsRejected => Report.Decision == 2;

    public async Task<IActionResult> OnGetAsync()
    {
        if (Search <= 0)
        {
            SearchMessage = "Enter a Deviation ID to search for its report.";
            return Page();
        }

        await LoadReportByDeviationAsync(Search);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Report.DeviationId <= 0)
        {
            ErrorMessage = "Load a report using its Deviation ID first.";
            return Page();
        }

        if (Decision != 1 && Decision != 2)
        {
            ModelState.AddModelError(
                nameof(Decision),
                "Please select Approve or Reject.");

            await LoadReportByDeviationAsync(Report.DeviationId);
            return Page();
        }

        // The current API does not expose an endpoint to save decisions.
        ErrorMessage =
            "The report was loaded, but the decision cannot be saved because the API does not currently provide an approval/rejection update endpoint.";

        await LoadReportByDeviationAsync(Report.DeviationId);

        return Page();
    }

    private async Task LoadReportByDeviationAsync(int deviationId)
    {
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl.TrimEnd('/') + "/")
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/DeviationReports/deviation/{deviationId}");

            // Forward the browser cookie to the API.
            if (Request.Headers.TryGetValue("Cookie", out var cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie",
                    cookie.ToString());
            }

            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ErrorMessage =
                    "The API returned 401 Unauthorized. Check that the browser request contains a valid QMS.Auth cookie and that the API can validate it.";

                _logger.LogWarning(
                    "API returned 401 for Deviation ID {DeviationId}",
                    deviationId);

                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage =
                    "Access denied. Your account may not have permission to view this report.";

                _logger.LogWarning(
                    "API returned 403 for Deviation ID {DeviationId}",
                    deviationId);

                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Report = new DeviationReportDto();
                SearchMessage = string.Empty;
                ErrorMessage =
                    $"No report was found for Deviation ID {deviationId}.";

                return;
            }

            response.EnsureSuccessStatusCode();

            var savedReport =
                await response.Content.ReadFromJsonAsync<DeviationReportRequest>();

            if (savedReport is null)
            {
                ErrorMessage = "The API returned no report data.";
                return;
            }

            MapReport(savedReport);

            Search = deviationId;
            SearchMessage =
                $"Report loaded successfully for Deviation ID {deviationId}.";
            ErrorMessage = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Could not load report for Deviation ID {DeviationId}",
                deviationId);

            ErrorMessage =
                $"Could not connect to the API at {ApiBaseUrl}. Verify that the API is running and the URL is correct.";
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error loading Deviation ID {DeviationId}",
                deviationId);

            ErrorMessage =
                "An unexpected error occurred while loading the report.";
        }
    }

    private void MapReport(DeviationReportRequest source)
    {
        Report = new DeviationReportDto
        {
            Id = source.Id,
            DeviationId = source.DeviationId,
            DocumentId = source.DocumentId,
            AttemptNumber = source.AttemptNumber,
            Summary = source.Summary,
            Proof = source.Proof,
            CreatedBy = source.CreatedBy,
            CreatedDate = source.CreatedDate,
            Status = source.Status,
            Decision = source.Decision,
            DecisionBy = source.DecisionBy ?? string.Empty,
            DecisionOn = source.DecisionOn,
            DecisionComments = source.DecisionComments ?? string.Empty
        };
    }
}