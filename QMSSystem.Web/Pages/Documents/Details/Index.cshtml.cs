using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Documents.Details;

public class IndexModel(
    IHttpClientFactory httpClientFactory,
    ILogger<IndexModel> logger) : PageModel
{
    public DocumentCreation? Document { get; private set; }

    public List<DocumentEvent> Events { get; private set; } = [];

    public List<DocumentHistoryDto> History { get; private set; } = [];

    public string? DocumentLoadError { get; private set; }

    public string? EventLogError { get; private set; }

    public bool HistoryLoadFailed { get; private set; }

    public async Task<IActionResult> OnGetAsync(
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

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Loading DocumentId {DocumentId} failed with status code {StatusCode}.",
                    id,
                    response.StatusCode);
                DocumentLoadError =
                    $"Document details could not be loaded (API returned HTTP {(int)response.StatusCode}).";
                return Page();
            }

            Document = await response.Content
                .ReadFromJsonAsync<DocumentCreation>(
                    cancellationToken: cancellationToken);

            if (Document is null)
            {
                logger.LogError("The document API returned an empty response for DocumentId {DocumentId}.", id);
                DocumentLoadError = "The document details could not be loaded. Please try again.";
                return Page();
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not connect to the API to load DocumentId {DocumentId}.", id);
            DocumentLoadError = "Could not connect to the API server. Check that it is running and try again.";
            return Page();
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "The document API returned invalid data for DocumentId {DocumentId}.", id);
            DocumentLoadError = "The document details could not be loaded because the API returned invalid data.";
            return Page();
        }

        await LoadEventsAsync(client, id, cancellationToken);
        await LoadHistoryAsync(client, id, cancellationToken);

        return Page();
    }

    private async Task LoadEventsAsync(
        HttpClient client,
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(
                $"api/Documents/{id}/events",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Loading the event log for DocumentId {DocumentId} failed with status code {StatusCode}.",
                    id,
                    response.StatusCode);
                EventLogError = response.StatusCode == HttpStatusCode.InternalServerError
                    ? "The event log is unavailable. Verify that the AddDocumentEvents database migration has been applied."
                    : $"The event log could not be loaded (API returned HTTP {(int)response.StatusCode}).";
                return;
            }

            Events = await response.Content
                .ReadFromJsonAsync<List<DocumentEvent>>(
                    cancellationToken: cancellationToken) ?? [];
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not load the event log for DocumentId {DocumentId}.", id);
            EventLogError = "Could not connect to the API server to load the event log.";
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "The event API returned invalid data for DocumentId {DocumentId}.", id);
            EventLogError = "The event log could not be loaded because the API returned invalid data.";
        }
    }

    private async Task LoadHistoryAsync(
        HttpClient client,
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await client.GetAsync(
                $"api/documenthistory/document/{id}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Loading version history for DocumentId {DocumentId} failed with status code {StatusCode}.",
                    id,
                    response.StatusCode);
                HistoryLoadFailed = true;
                return;
            }

            History = await response.Content
                .ReadFromJsonAsync<List<DocumentHistoryDto>>(
                    cancellationToken: cancellationToken) ?? [];
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not load version history for DocumentId {DocumentId}.", id);
            HistoryLoadFailed = true;
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "The history API returned invalid data for DocumentId {DocumentId}.", id);
            HistoryLoadFailed = true;
        }
    }
}
