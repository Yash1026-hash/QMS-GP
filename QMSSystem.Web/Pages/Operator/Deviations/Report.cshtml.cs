using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public sealed class ReportModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ReportModel> _logger;

    public ReportModel(
        IHttpClientFactory httpClientFactory,
        ILogger<ReportModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public int Id { get; } = 0;

    [BindProperty(SupportsGet = true)]
    public int? DeviationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DocumentId { get; set; }

    public int AttemptNumber { get; private set; } = 1;

    [BindProperty]
    public string Summary { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Proof { get; set; }

    public string CreatedBy { get; private set; } = "Anonymous";

    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    public int Status { get; private set; }

    public void OnGet()
    {
        CreatedBy = User.Identity?.IsAuthenticated == true
            ? User.Identity.Name ?? "Unknown"
            : "Anonymous";
        CreatedDate = DateTime.UtcNow;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DeviationId is not > 0)
        {
            ModelState.AddModelError(
                nameof(DeviationId),
                "Enter a valid accepted deviation ID.");
        }

        if (DocumentId is not > 0)
        {
            ModelState.AddModelError(
                nameof(DocumentId),
                "Enter a valid SOP document ID.");
        }

        if (string.IsNullOrWhiteSpace(Summary))
        {
            ModelState.AddModelError(
                nameof(Summary),
                "Enter a summary of the corrective work.");
        }

        if (Proof is null || Proof.Length == 0)
        {
            ModelState.AddModelError(
                nameof(Proof),
                "Choose a non-empty proof file.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        byte[] proofBytes;
        try
        {
            await using var proofStream = new MemoryStream();
            await Proof!.CopyToAsync(proofStream);
            proofBytes = proofStream.ToArray();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Could not read the uploaded proof for a deviation report.");
            ModelState.AddModelError(
                nameof(Proof),
                "The proof file could not be read. Please choose it again and retry.");
            return Page();
        }

        var report = new DeviationReportDto
        {
            DeviationId = DeviationId!.Value,
            DocumentId = DocumentId!.Value,
            AttemptNumber = AttemptNumber,
            Summary = Summary.Trim(),
            Proof = proofBytes,
            CreatedBy = User.Identity?.Name ?? "Anonymous",
            CreatedDate = DateTime.UtcNow,
            Status = 0
        };

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            using var response = await client.PostAsJsonAsync(
                "api/DeviationReport",
                report);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = await response.Content.ReadAsStringAsync();
                _logger.LogWarning(
                    "Could not submit deviation report. API returned {StatusCode}: {ApiError}",
                    response.StatusCode,
                    apiError);

                ModelState.AddModelError(
                    string.Empty,
                    "The report could not be submitted. Check the deviation and document IDs, then try again.");
                return Page();
            }

            var savedReport =
                await response.Content.ReadFromJsonAsync<DeviationReportDto>();

            if (savedReport is null || savedReport.Id <= 0)
            {
                _logger.LogWarning(
                    "The deviation report API returned a successful response without a valid report ID.");
                ModelState.AddModelError(
                    string.Empty,
                    "The API did not confirm that the report was saved. Please try again.");
                return Page();
            }

            TempData["SuccessMessage"] =
                $"Deviation report {savedReport.Id} was submitted successfully.";
            return RedirectToPage(
                new
                {
                    deviationId = DeviationId,
                    documentId = DocumentId
                });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Could not connect to the QMS API to submit a deviation report.");
            ModelState.AddModelError(
                string.Empty,
                "Could not connect to the QMS API. Check that the API is running and try again.");
            return Page();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "The QMS API returned an invalid response while submitting a deviation report.");
            ModelState.AddModelError(
                string.Empty,
                "The QMS API returned an invalid response. The report could not be confirmed.");
            return Page();
        }
    }
}
