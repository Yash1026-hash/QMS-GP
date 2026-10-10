using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Documents;

public class ApprovalModel(
    IHttpClientFactory httpClientFactory,
    ILogger<ApprovalModel> logger) : PageModel
{
    public List<DocumentCreation> Documents { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var loadResult = await LoadDocumentsAsync(cancellationToken);
        return loadResult ?? Page();
    }

    public async Task<IActionResult> OnPostDecideAsync(
        int documentId,
        string decision,
        string comment,
        CancellationToken cancellationToken)
    {
        if (documentId <= 0 ||
            !new[] { "Approve", "Return", "Reject" }.Contains(decision, StringComparer.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(comment))
        {
            ErrorMessage = "Select a decision and enter a review comment.";
            await LoadDocumentsAsync(cancellationToken);
            return Page();
        }

        try
        {
            var client = httpClientFactory.CreateClient("QMSApi");
            using var response = await client.PostAsJsonAsync(
                $"api/Documents/{documentId}/decision",
                new DocumentDecisionRequest
                {
                    Decision = decision,
                    Comment = comment.Trim()
                },
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadAsStringAsync(cancellationToken);
                ErrorMessage = string.IsNullOrWhiteSpace(details)
                    ? $"The API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})."
                    : details;
                await LoadDocumentsAsync(cancellationToken);
                return Page();
            }

            StatusMessage = decision.Equals("Approve", StringComparison.OrdinalIgnoreCase)
                ? "Document approved and activated."
                : decision.Equals("Return", StringComparison.OrdinalIgnoreCase)
                    ? "Document returned to its creator for changes."
                    : "Document rejected.";

            return RedirectToPage();
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Failed to submit document decision for {DocumentId}.", documentId);
            ErrorMessage = "Could not connect to the API server. Check that it is running and try again.";
            await LoadDocumentsAsync(cancellationToken);
            return Page();
        }
    }

    private async Task<IActionResult?> LoadDocumentsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("QMSApi");
            using var response = await client.GetAsync("api/Documents", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Could not load documents. The API returned HTTP {(int)response.StatusCode}.";
                return null;
            }

            var documents = await response.Content
                .ReadFromJsonAsync<List<DocumentCreation>>(cancellationToken);

            Documents = (documents ?? [])
                .Where(document =>
                    document.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
                    document.Status.Equals("Returned", StringComparison.OrdinalIgnoreCase))
                .OrderBy(document => document.CreationOn)
                .ToList();

            return null;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Failed to load documents for review.");
            ErrorMessage = "Could not connect to the API server. Check that it is running and try again.";
            return null;
        }
    }
}
