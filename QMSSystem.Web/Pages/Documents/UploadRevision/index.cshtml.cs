using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using QMSSystem.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace QMSSystem.Web.Pages.Documents.UploadRevision;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Document Document { get; set; } = null!;

    [BindProperty, Required, StringLength(50)]
    public string DocumentNumber { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;

    [BindProperty, Required, StringLength(2000)]
    public string Comment { get; set; } = string.Empty;

    [BindProperty] public IFormFile? UploadFile { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        var response = await client.GetAsync($"api/Documents/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();

        var document = await response.Content.ReadFromJsonAsync<Document>();
        if (document is null) return NotFound();

        Document = document;
        SetEditableFields(document);
        return Page();
    }

    public async Task<IActionResult> OnGetCurrentFileAsync(int id, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");
        using var response = await client.GetAsync(
            $"api/Documents/{id}/current-file",
            cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode);
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "current-document";
        fileName = Path.GetFileName(fileName.Trim('"'));
        var contentType = response.Content.Headers.ContentType?.ToString()
            ?? "application/octet-stream";

        return File(content, contentType, fileName);
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        // Reload doc for redisplay if validation fails
        var getResp = await client.GetAsync($"api/Documents/{id}", cancellationToken);
        if (!getResp.IsSuccessStatusCode) return NotFound();
        var document = await getResp.Content.ReadFromJsonAsync<Document>();
        if (document is null) return NotFound();

        Document = document;

        if (UploadFile is null || UploadFile.Length == 0)
        {
            ModelState.AddModelError(nameof(UploadFile), "Please choose a file.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var uploadFile = UploadFile
            ?? throw new InvalidOperationException("Please choose a file.");

        using var content = new MultipartFormDataContent();
        using var stream = uploadFile.OpenReadStream();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                uploadFile.ContentType ?? "application/octet-stream");

        content.Add(fileContent, "file", uploadFile.FileName);
        content.Add(new StringContent(DocumentNumber.Trim()), "documentNumber");
        content.Add(new StringContent(Title.Trim()), "title");
        content.Add(new StringContent(Department.Trim()), "department");
        content.Add(new StringContent(Comment.Trim()), "comment");

        using var response = await client.PostAsync(
            $"api/Documents/{id}/upload-revision",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            ModelState.AddModelError("", $"Upload failed ({response.StatusCode}): {errorBody}");
            return Page();
        }

        return RedirectToPage("/Documents/Index");
    }

    private void SetEditableFields(Document document)
    {
        DocumentNumber = document.DocumentNumber;
        Title = document.Title;
        Department = document.Department;
        Comment = document.Comment ?? string.Empty;
    }
}