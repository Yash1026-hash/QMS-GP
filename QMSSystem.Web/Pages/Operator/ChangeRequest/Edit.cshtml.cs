using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Operator.ChangeRequest;

public class EditModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EditModel> _logger;

    public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty]
    public int DocumentId { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string ChangeType { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public int RequestedByUserId { get; set; } = 2; // Operator default

    [BindProperty]
    public DateTime RequestedDate { get; set; } = DateTime.Today;

    [BindProperty]
    public List<int> SelectedDeviationIds { get; set; } = [];

    public List<DocumentCreation> ActiveDocuments { get; set; } = [];
    public List<DeviationRequestDto> Deviations { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int? documentId)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        // Fetch active/approved documents
        try
        {
            var docsResponse = await client.GetAsync("api/ChangeRequestDocument/active");
            if (docsResponse.IsSuccessStatusCode)
            {
                ActiveDocuments = await docsResponse.Content.ReadFromJsonAsync<List<DocumentCreation>>() ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active documents for change request");
            ActiveDocuments = [];
        }

        if (documentId.HasValue && documentId.Value > 0)
        {
            DocumentId = documentId.Value;
        }
        else if (ActiveDocuments.Count > 0)
        {
            DocumentId = ActiveDocuments[0].Id;
        }

        if (DocumentId > 0)
        {
            await LoadDeviationsForDocumentAsync(DocumentId);
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        if (DocumentId <= 0)
        {
            ModelState.AddModelError(nameof(DocumentId), "Please select a valid document.");
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError(nameof(Title), "Title is required.");
        }

        if (string.IsNullOrWhiteSpace(ChangeType))
        {
            ModelState.AddModelError(nameof(ChangeType), "Change type is required.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Description is required.");
        }

        if (!ModelState.IsValid)
        {
            await ReloadFormDependenciesAsync();
            return Page();
        }

        var request = new ChangeRequestForm
        {
            DocumentId = DocumentId,
            Title = Title.Trim(),
            ChangeType = ChangeType,
            Description = Description.Trim(),
            RequestedByUserId = RequestedByUserId > 0 ? RequestedByUserId : 2,
            RequestedDate = DateTime.UtcNow,
            Status = 0,
            SelectedDeviationIds = SelectedDeviationIds ?? []
        };

        try
        {
            var response = await client.PostAsJsonAsync("api/ChangeRequestDocument/FormSubmission", request);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = "Failed to submit change request to server.";
                await ReloadFormDependenciesAsync();
                return Page();
            }

            TempData["SuccessMessage"] = $"Change request '{request.Title}' submitted successfully.";
            return RedirectToPage("/Operator/ChangeRequest/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit change request");
            ErrorMessage = "Could not communicate with the API server.";
            await ReloadFormDependenciesAsync();
            return Page();
        }
    }

    private async Task LoadDeviationsForDocumentAsync(int docId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var response = await client.GetAsync($"api/ChangeRequestDocument/{docId}/deviations");
            if (response.IsSuccessStatusCode)
            {
                Deviations = await response.Content.ReadFromJsonAsync<List<DeviationRequestDto>>() ?? [];
            }
        }
        catch
        {
            Deviations = [];
        }
    }

    private async Task ReloadFormDependenciesAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var docsResponse = await client.GetAsync("api/ChangeRequestDocument/active");
            if (docsResponse.IsSuccessStatusCode)
            {
                ActiveDocuments = await docsResponse.Content.ReadFromJsonAsync<List<DocumentCreation>>() ?? [];
            }
        }
        catch
        {
            ActiveDocuments = [];
        }

        if (DocumentId > 0)
        {
            await LoadDeviationsForDocumentAsync(DocumentId);
        }
    }
}