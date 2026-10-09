
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Supervisor;

public class DeviationReviewReportModel : PageModel
{
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

    [BindProperty(SupportsGet = true)]
    public int ReportId { get; set; }

    public List<DeviationReportDto> Reports { get; private set; } = [];

    public string ErrorMessage { get; private set; } = string.Empty;
    public bool IsSubmitted { get; private set; }

    public string DecisionMessage { get; private set; } = string.Empty;

    public bool IsApproved => Report.Decision == 1;

    public bool IsRejected => Report.Decision == 2;

    public async Task<IActionResult> OnGetAsync() 
    {
        if (ReportId <= 0)
        {
            await LoadReportsAsync();
            return Page();
        }

        await LoadReportByIdAsync(ReportId);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        int deviationId = Report.DeviationId;
        int reportId = Report.Id;
        ReportId = reportId;

        if (deviationId <= 0 || reportId <= 0)
        {
            ErrorMessage = "Select a report from the submitted reports list first.";
            return Page();
        }

        if (Decision != 1 && Decision != 2)
        {
            await LoadReportByIdAsync(reportId);
            ErrorMessage = "Please select Approve or Reject.";
            return Page();
        }

        int decision = Decision.Value;

        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl.TrimEnd('/') + "/")
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Put,
                $"api/DeviationReports/{reportId}/decision")
            {
                Content = JsonContent.Create(new DeviationReportDecisionDto
                {
                    Decision = decision,
                    DecisionComments = DecisionComments
                })
            };

            if (Request.Headers.TryGetValue("Cookie", out var cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie.ToString());
            }

            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = response.StatusCode switch
                {
                    HttpStatusCode.Unauthorized =>
                        "The API returned 401 Unauthorized. Check the authentication cookie.",
                    HttpStatusCode.Forbidden =>
                        "Access denied. A supervisor account is required to review reports.",
                    HttpStatusCode.NotFound =>
                        "The report could not be found.",
                    HttpStatusCode.Conflict =>
                        "This report has already been reviewed.",
                    _ => $"Could not save the decision (HTTP {(int)response.StatusCode})."
                };

                await LoadReportByIdAsync(reportId);
                ErrorMessage = errorMessage;
                return Page();
            }

            await LoadReportByIdAsync(reportId);
            if (Report.Id <= 0 ||
                Report.Decision != decision ||
                !string.Equals(
                    Report.DecisionComments,
                    DecisionComments,
                    StringComparison.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(ErrorMessage))
                {
                    ErrorMessage = "The decision request succeeded, but the saved report could not be confirmed.";
                }

                return Page();
            }

            DecisionMessage = decision == 1
                ? "Report approved and saved successfully."
                : "Report rejected and saved successfully.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Could not save decision for report {ReportId}",
                reportId);

            await LoadReportByIdAsync(reportId);
            ErrorMessage =
                $"Could not connect to the API at {ApiBaseUrl}. Check that the API is running.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error saving decision for report {ReportId}",
                reportId);

            await LoadReportByIdAsync(reportId);
            ErrorMessage = "An unexpected error occurred while saving the decision.";
        }

        return Page();
    }

    private async Task LoadReportsAsync()
    {
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl.TrimEnd('/') + "/")
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/DeviationReports");

            if (Request.Headers.TryGetValue("Cookie", out var cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie.ToString());
            }

            using var response = await client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "The API returned 401 Unauthorized. Check the authentication cookie.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage = "Access denied. Your account may not have permission.";
                return;
            }

            response.EnsureSuccessStatusCode();

            var savedReports =
                await response.Content.ReadFromJsonAsync<List<DeviationReportRequest>>();

            if (savedReports is null)
            {
                ErrorMessage = "The API returned no report data.";
                return;
            }

            Reports = savedReports
                .Select(MapReport)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not load submitted deviation reports");
            ErrorMessage =
                $"Could not connect to the API at {ApiBaseUrl}. Check that the API is running.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading submitted deviation reports");
            ErrorMessage = "An unexpected error occurred while loading submitted reports.";
        }
    }

    private async Task LoadReportByIdAsync(int reportId)
    {
        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl.TrimEnd('/') + "/")
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/DeviationReports/{reportId}");

            if (Request.Headers.TryGetValue("Cookie", out var cookie))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie", cookie.ToString());
            }

            using var response = await client.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "The API returned 401 Unauthorized. Check the authentication cookie.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage = "Access denied. Your account may not have permission.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Report = new DeviationReportDto();
                ErrorMessage = $"Deviation report {reportId} was not found.";
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

            Report = MapReport(savedReport);
            ErrorMessage = string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex,
                "Could not load report {ReportId}",
                reportId);

            ErrorMessage =
                $"Could not connect to the API at {ApiBaseUrl}. Check that the API is running.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error loading report {ReportId}",
                reportId);

            ErrorMessage = "An unexpected error occurred while loading the report.";
        }
    }

    private static DeviationReportDto MapReport(DeviationReportRequest source)
    {
        return new DeviationReportDto
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