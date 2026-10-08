using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Documents;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CreateModel(IHttpClientFactory httpClientFactory)
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (DocumentFile == null || DocumentFile.Length == 0)
        {
            ModelState.AddModelError("DocumentFile", "Please select a file.");
            return Page();
        }

        // Convert uploaded file → byte[]
        using var stream = new MemoryStream();

        await DocumentFile.CopyToAsync(stream);

        Document.FileData = stream.ToArray();
        Document.FileName = DocumentFile.FileName;
        Document.ContentType = DocumentFile.ContentType;

        // Example for now
        Document.CreatedBy = 1;

        var client = _httpClientFactory.CreateClient("QMSApi");

        var response = await client.PostAsJsonAsync(
            "api/Documents",
            Document);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            ModelState.AddModelError(
                string.Empty,
                $"Failed to create document: {error}");

            return Page();
        }

        return RedirectToPage("/Documents/Index");
    }
}