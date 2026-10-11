using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public class DetailsModel(
    IHttpClientFactory httpClientFactory,
    ILogger<DetailsModel> logger) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public DeviationRequestDto? Deviation { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string DocumentNumber { get; private set; } = "Not available";

    public string DocumentTitle { get; private set; } = "Not available";

    public string StatusText => Deviation?.Decision switch
    {
        1 => "Approved",
        2 => "Rejected",
        _ => Deviation?.Status switch
        {
            0 => "Pending review",
            1 => "Active",
            2 => "Inactive",
            _ => "Unknown"
        }
    };

    public string StatusCssClass => Deviation?.Decision switch
    {
        1 => "approved",
        2 => "rejected",
        _ => Deviation?.Status switch
        {
            0 => "pending",
            1 => "approved",
            2 => "rejected",
            _ => "unknown"
        }
    };

    public async Task OnGetAsync()
    {
        if (Id <= 0)
        {
            return;
        }

        try
        {
            var client = httpClientFactory.CreateClient("ApiClient");
            using var response = await client.GetAsync($"/api/Deviation/{Id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                ErrorMessage = $"Deviation {Id} was not found.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                ErrorMessage = "Your session has expired. Sign in again to view this deviation.";
                return;
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                ErrorMessage = "You are not authorized to view this deviation.";
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "API returned HTTP {StatusCode} while loading deviation {DeviationId}",
                    (int)response.StatusCode,
                    Id);
                ErrorMessage = $"The API could not load deviation {Id} (HTTP {(int)response.StatusCode}).";
                return;
            }

            Deviation = await response.Content.ReadFromJsonAsync<DeviationRequestDto>();
            if (Deviation is null)
            {
                ErrorMessage = "The API returned no deviation details.";
                return;
            }

            await LoadDocumentDetailsAsync(client, Deviation.DocumentId);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Could not load deviation {DeviationId}", Id);
            ErrorMessage = "Could not connect to the API while loading this deviation.";
        }
    }

    public string DisplayValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Not provided" : value;

    private async Task LoadDocumentDetailsAsync(HttpClient client, int documentId)
    {
        try
        {
            using var response = await client.GetAsync($"/api/Documents/{documentId}/summary");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                DocumentNumber = "Document not found";
                DocumentTitle = "Document not found";
                return;
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "API returned HTTP {StatusCode} while loading document {DocumentId}",
                    (int)response.StatusCode,
                    documentId);
                DocumentNumber = "Unable to load document";
                DocumentTitle = "Unable to load document";
                return;
            }
            var document = await response.Content.ReadFromJsonAsync<DocumentDetails>();
            if (document is not null)
            {
                DocumentNumber = DisplayValue(document.DocumentNumber);
                DocumentTitle = DisplayValue(document.Title);
            }
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception,
                "Could not load document {DocumentId} for deviation {DeviationId}",
                documentId,
                Id);
            DocumentNumber = "Unable to load document";
            DocumentTitle = "Unable to load document";
        }
    }

    private sealed class DocumentDetails
    {
        public string DocumentNumber { get; init; } = string.Empty;

        public string Title { get; init; } = string.Empty;
    }
}