using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMS.Pages.Documents;

public class CreateModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty]
    public Document Document { get; set; } = new();

    [BindProperty]
    public IFormFile? DocumentFile { get; set; }

    [BindProperty]
    public string? Comment { get; set; }

    public void OnGet()
    {
        Document.CurrentVersion = 0;
        Document.Status = "Draft";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (DocumentFile == null || DocumentFile.Length == 0)
        {
            ModelState.AddModelError("DocumentFile", "Please select a document.");
        }

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

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId) || userId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Sign in before creating a document.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var client = _httpClientFactory.CreateClient("QMSApi");
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(Document.DocumentNumber.Trim()), "documentNumber");
        content.Add(new StringContent(Document.Title.Trim()), "title");
        content.Add(new StringContent(Document.Department.Trim()), "department");
        content.Add(new StringContent(userId.ToString()), "createdBy");

        if (!string.IsNullOrWhiteSpace(Comment))
        {
            content.Add(new StringContent(Comment), "comment");
        }

        await using var fileStream = DocumentFile!.OpenReadStream();
        using var fileContent = new StreamContent(fileStream);
        if (System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(
                DocumentFile.ContentType,
                out var contentType))
        {
            fileContent.Headers.ContentType = contentType;
        }

        content.Add(fileContent, "file", Path.GetFileName(DocumentFile.FileName));

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsync("api/documents", content, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "The document API could not be reached.");
            ModelState.AddModelError(string.Empty, "Unable to reach the document service. Ensure the API is running and try again.");
            return Page();
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var message = response.StatusCode == System.Net.HttpStatusCode.Conflict
                    ? "A document with this number already exists."
                    : $"Unable to create document (API returned {(int)response.StatusCode}).";
                ModelState.AddModelError(string.Empty, message);

                return Page();
            }
        }

        return RedirectToPage("/Index");
    }
}