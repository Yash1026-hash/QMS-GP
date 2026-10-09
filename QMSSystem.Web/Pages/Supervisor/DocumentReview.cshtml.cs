using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Supervisor;

public class DocumentReviewModel(
    IHttpClientFactory httpClientFactory,
    ILogger<DocumentReviewModel> logger) : PageModel
{
    private static readonly int[] AllowedPageSizes = [10, 25, 50, 100];

    public IReadOnlyList<Document> Documents { get; private set; } = [];

    public int PageNumber { get; private set; } = 1;

    public int PageSize { get; private set; } = 10;

    public IReadOnlyList<int> PageSizeOptions => AllowedPageSizes;

    public string? DocumentNumber { get; private set; }

    public int TotalCount { get; private set; }

    public int TotalPages { get; private set; }

    public string? LoadErrorMessage { get; private set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(
        int page = 1,
        int pageSize = 10,
        string? documentNumber = null,
        CancellationToken cancellationToken = default)
    {
        PageNumber = Math.Max(page, 1);
        PageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10;
        DocumentNumber = documentNumber?.Trim();
        var client = httpClientFactory.CreateClient("QMSApi");

        try
        {
            using var response = await client.GetAsync(
                $"api/supervisor/documents/pending?page={PageNumber}&pageSize={PageSize}&documentNumber={Uri.EscapeDataString(DocumentNumber ?? string.Empty)}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Loading pending documents failed with status code {StatusCode}.",
                    response.StatusCode);
                LoadErrorMessage =
                    "Pending documents could not be loaded. Please try again.";
                return;
            }

            var result = await response.Content
                .ReadFromJsonAsync<PendingDocumentsResponse>(
                    cancellationToken: cancellationToken);

            if (result is null)
            {
                logger.LogError("The pending documents API returned an empty response.");
                LoadErrorMessage =
                    "Pending documents could not be loaded. Please try again.";
                return;
            }

            Documents = result.Documents;
            TotalCount = result.TotalCount;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            PageNumber = result.Page;
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not connect to the API to load pending documents.");
            LoadErrorMessage =
                "Pending documents could not be loaded. Please try again.";
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "The pending documents API returned invalid data.");
            LoadErrorMessage =
                "Pending documents could not be loaded. Please try again.";
        }
    }

    public sealed class PendingDocumentsResponse
    {
        public List<Document> Documents { get; init; } = [];

        public int TotalCount { get; init; }

        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 10;
    }

    public async Task<IActionResult> OnGetPreviewAsync(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return NotFound();
        }

        var client = httpClientFactory.CreateClient("QMSApi");

        try
        {
            using var response = await client.GetAsync(
                $"api/Documents/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Loading preview failed for DocumentId {DocumentId} with status code {StatusCode}.",
                    id,
                    response.StatusCode);
                return response.StatusCode == System.Net.HttpStatusCode.NotFound
                    ? NotFound()
                    : StatusCode(StatusCodes.Status502BadGateway);
            }

            var document = await response.Content
                .ReadFromJsonAsync<DocumentCreation>(
                    cancellationToken: cancellationToken);

            if (document is null || document.FileData.Length == 0)
            {
                logger.LogWarning(
                    "DocumentId {DocumentId} has no file data to preview.",
                    id);
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(document.ContentType))
            {
                logger.LogWarning(
                    "DocumentId {DocumentId} has no content type for preview.",
                    id);
                return BadRequest("This document cannot be previewed.");
            }

            Response.Headers["Content-Disposition"] = "inline";
            Response.Headers["Content-Security-Policy"] = "sandbox";
            Response.Headers["X-Content-Type-Options"] = "nosniff";

            return File(document.FileData, document.ContentType);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "Could not load preview for DocumentId {DocumentId}.",
                id);
            return StatusCode(StatusCodes.Status502BadGateway);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "The document preview API returned invalid data for DocumentId {DocumentId}.",
                id);
            return StatusCode(StatusCodes.Status502BadGateway);
        }
    }

    public async Task<IActionResult> OnPostDecisionAsync(
        int documentId,
        string? decision,
        string? comments,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        string? documentNumber = null)
    {
        var returnRoute = new
        {
            page = Math.Max(page, 1),
            pageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10,
            documentNumber = documentNumber?.Trim()
        };

        if (documentId <= 0)
        {
            ErrorMessage = "Select a valid document.";
            return RedirectToPage("/Supervisor/DocumentReview", returnRoute);
        }

        if (string.IsNullOrWhiteSpace(decision) ||
            decision.Trim().ToLowerInvariant() is not ("approve" or "revision" or "reject"))
        {
            ErrorMessage = "Select a valid approval decision.";
            return RedirectToPage("/Supervisor/DocumentReview", returnRoute);
        }

        if (string.IsNullOrWhiteSpace(comments) || comments.Trim().Length > 1000)
        {
            ErrorMessage = "Enter reviewer comments (up to 1000 characters).";
            return RedirectToPage("/Supervisor/DocumentReview", returnRoute);
        }

        var client = httpClientFactory.CreateClient("QMSApi");
        var request = new Document
        {
            Id = documentId,
            Decision = decision.Trim(),
            Comment = comments.Trim()
        };

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/supervisor/documents/decision",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Saving document decision failed for DocumentId {DocumentId} with status {StatusCode}. Response: {ResponseBody}",
                    documentId,
                    response.StatusCode,
                    responseBody);

                ErrorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "The selected document was not found.",
                    System.Net.HttpStatusCode.Conflict => "This document is no longer pending review.",
                    _ => "The document decision could not be saved. Please try again."
                };
                return RedirectToPage("/Supervisor/DocumentReview", returnRoute);
            }

        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "Could not submit decision for DocumentId {DocumentId}.",
                documentId);
            ErrorMessage = "Could not connect to the API. Please try again.";
        }

        return RedirectToPage("/Supervisor/DocumentReview", returnRoute);
    }
}