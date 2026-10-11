using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Text.Json;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Operator.ChangeRequest;

[AllowAnonymous]
public class DocumentsModel : PageModel
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DocumentsModel> _logger;

    public DocumentsModel(IHttpClientFactory factory, ILogger<DocumentsModel> logger)
    {
        _httpClient = factory.CreateClient("QMSApi");
        _logger = logger;
    }

    public List<DocumentCreation> Documents { get; private set; } = new();

    public int TotalCount => Documents.Count;

    public async Task OnGetAsync()
    {
        try
        {
            Documents = await _httpClient
                .GetFromJsonAsync<List<DocumentCreation>>
                ("api/ChangeRequestDocument/active") ?? [];
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Failed to load active change request documents.");
            Documents = [];
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "The active change request documents API returned invalid data.");
            Documents = [];
        }
    }
}