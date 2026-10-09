using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Operator.ChangeRequest;

[AllowAnonymous]
public class DocumentsModel : PageModel
{
    private readonly HttpClient _httpClient;

    public DocumentsModel(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("QMSApi");
    }

    public List<Document> Documents { get; private set; } = new();

    public int TotalCount => Documents.Count;

    public async Task OnGetAsync()
    {
        try
        {
            Documents = await _httpClient
                .GetFromJsonAsync<List<Document>>
                ("api/ChangeRequestDocument/active")
                ?? new List<Document>();
        }
        catch
        {
            Documents = new List<Document>();
        }
    }
}