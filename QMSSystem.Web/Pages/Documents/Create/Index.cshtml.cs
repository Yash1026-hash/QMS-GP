using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

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
        Document.DocumentVersion = 0;
        Document.Status = "Pending";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
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

        using var stream = new MemoryStream();
        await DocumentFile!.CopyToAsync(stream, cancellationToken);

        Document.FileData = stream.ToArray();
        Document.FileName = DocumentFile.FileName;
        Document.ContentType = DocumentFile.ContentType;
        Document.DocumentVersion = 0;
        Document.Status = "Pending";
        Document.CreatedBy = 1;
        Document.CreationOn = DateTime.UtcNow;

        var client = _httpClientFactory.CreateClient("QMSApi");

        try
        {
            using var response = await client.PostAsJsonAsync(
                "api/Documents",
                Document,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                var message = string.IsNullOrWhiteSpace(error)
                    ? $"The API returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase})."
                    : $"The API returned HTTP {(int)response.StatusCode}: {error}";

                ModelState.AddModelError(string.Empty, $"Failed to create document. {message}");
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

        return RedirectToPage("/Index");
    }
}