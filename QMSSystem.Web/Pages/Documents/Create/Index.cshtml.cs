using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace QMSSystem.Web.Pages.Documents.Create;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public DocumentCreation Document { get; set; } = new();

    [BindProperty]
    public IFormFile? DocumentFile { get; set; }

    public void OnGet()
    {
        Document.DocumentVersion = 1;
        Document.Status = "Pending";
    }

    public async Task<IActionResult> OnGetCheckNumberAsync(string documentNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return new JsonResult(new CheckNumberResult { Exists = false });
        }

        var client = _httpClientFactory.CreateClient("QMSApi");
        try
        {
            var response = await client.GetAsync(
                $"api/Documents/check-number?documentNumber={Uri.EscapeDataString(documentNumber.Trim())}",
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CheckNumberResult>(cancellationToken: cancellationToken);
                return new JsonResult(result ?? new CheckNumberResult { Exists = false });
            }
        }
        catch
        {
            // Fall through on network error
        }

        return new JsonResult(new CheckNumberResult { Exists = false });
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Document.DocumentNumber = Document.DocumentNumber?.Trim() ?? string.Empty;
        Document.Title = Document.Title?.Trim() ?? string.Empty;
        Document.Department = Document.Department?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(Document.DocumentNumber))
        {
            ModelState.AddModelError("Document.DocumentNumber", "Enter a document number.");
        }

        if (string.IsNullOrWhiteSpace(Document.Title))
        {
            ModelState.AddModelError("Document.Title", "Enter a document title.");
        }

        if (string.IsNullOrWhiteSpace(Document.Department))
        {
            ModelState.AddModelError("Document.Department", "Enter a department.");
        }

        if (DocumentFile == null || DocumentFile.Length == 0)
        {
            ModelState.AddModelError("DocumentFile", "Please select a file.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("QMSApi");

        // Server-side uniqueness verification before upload
        try
        {
            var checkResp = await client.GetAsync(
                $"api/Documents/check-number?documentNumber={Uri.EscapeDataString(Document.DocumentNumber)}",
                cancellationToken);

            if (checkResp.IsSuccessStatusCode)
            {
                var checkResult = await checkResp.Content.ReadFromJsonAsync<CheckNumberResult>(cancellationToken: cancellationToken);
                if (checkResult?.Exists == true)
                {
                    ModelState.AddModelError(
                        "Document.DocumentNumber",
                        $"Document number '{Document.DocumentNumber}' already exists. Document number must be unique.");
                    return Page();
                }
            }
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Could not connect to the API server. Check that the API is running and try again.");
            return Page();
        }

        using var stream = new MemoryStream();
        await DocumentFile!.CopyToAsync(stream, cancellationToken);

        Document.FileData = stream.ToArray();
        Document.FileName = DocumentFile.FileName;
        Document.ContentType = DocumentFile.ContentType;
        Document.DocumentVersion = 1;
        Document.Status = "Pending";
        Document.CreatedBy = 1;
        Document.CreationOn = DateTime.UtcNow;

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/Documents",
                Document,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                if (error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        "Document.DocumentNumber",
                        $"Document number '{Document.DocumentNumber}' already exists. Document number must be unique.");
                }
                else
                {
                    var message = string.IsNullOrWhiteSpace(error)
                        ? $"The API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})."
                        : $"The API returned HTTP {(int)response.StatusCode}: {error}";

                    ModelState.AddModelError(string.Empty, $"Failed to create document. {message}");
                }
                return Page();
            }
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "Could not connect to the API server. Check that the API is running and try again.");
            return Page();
        }

        return RedirectToPage("/Documents/Index");
    }

    public class CheckNumberResult
    {
        [JsonPropertyName("exists")]
        public bool Exists { get; set; }

        [JsonPropertyName("documentNumber")]
        public string? DocumentNumber { get; set; }
    }
}