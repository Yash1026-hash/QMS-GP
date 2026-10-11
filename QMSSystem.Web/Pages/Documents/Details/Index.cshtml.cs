using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Documents;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DetailsModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Document? Document { get; set; }
    public List<DocumentHistoryDto> History { get; set; } = [];
    public bool HistoryLoadFailed { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("QMSApi");

        using var response = await client.GetAsync(
            $"api/Documents/{id}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return NotFound();
        }

        Document = await response.Content
            .ReadFromJsonAsync<Document>();

        if (Document == null)
        {
            return NotFound();
        }

        using var historyResponse = await client.GetAsync(
            $"api/documenthistory/document/{id}",
            cancellationToken);

        if (historyResponse.IsSuccessStatusCode)
        {
            History = await historyResponse.Content
                .ReadFromJsonAsync<List<DocumentHistoryDto>>(
                    cancellationToken: cancellationToken) ?? [];
        }
        else
        {
            HistoryLoadFailed = true;
        }

        return Page();
    }

    public async Task<IActionResult> OnGetPreviewAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();

        var client = _httpClientFactory.CreateClient("QMSApi");
        using var response = await client.GetAsync($"api/Documents/{id}/current-file?inline=true", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode == System.Net.HttpStatusCode.NotFound ? NotFound() : StatusCode((int)response.StatusCode);
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";
        var fileName = response.Content.Headers.ContentDisposition?.FileName?.Trim('"') ?? "document";

        Response.Headers["Content-Disposition"] = $"inline; filename=\"{fileName}\"";
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        return File(content, contentType);
    }

    public async Task<IActionResult> OnGetDownloadAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0) return NotFound();

        var client = _httpClientFactory.CreateClient("QMSApi");
        using var response = await client.GetAsync($"api/Documents/{id}/current-file", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode == System.Net.HttpStatusCode.NotFound ? NotFound() : StatusCode((int)response.StatusCode);
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName
            ?? "document";
        fileName = Path.GetFileName(fileName.Trim('"'));
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";

        return File(content, contentType, fileName);
    }
}