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
}