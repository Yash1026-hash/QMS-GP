using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin.DocumentRegister;

public class DetailsModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Document? Document { get; set; }

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return RedirectToPage("/Admin/DocumentRegister/Index");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            Document = await client.GetFromJsonAsync<Document>($"api/admin/documents/{Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load document details for ID {Id}", Id);
            ErrorMessage = $"Unable to load document #{Id} from API.";
        }

        return Page();
    }
}
