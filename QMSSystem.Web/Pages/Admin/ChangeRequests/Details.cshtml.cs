using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin.ChangeRequests;

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

    public OperatorChangeRequest? ChangeRequest { get; set; }

    public List<OperatorChangeRequestDeviation> LinkedDeviations { get; set; } = [];

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return RedirectToPage("/Admin/ChangeRequests/Index");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var result = await client.GetFromJsonAsync<AdminChangeRequestDetailDto>($"api/admin/change-requests/{Id}");

            if (result != null)
            {
                ChangeRequest = result.ChangeRequest;
                LinkedDeviations = result.LinkedDeviations ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load change request details for ID {Id}", Id);
            ErrorMessage = $"Unable to load change request #{Id} from API.";
        }

        return Page();
    }
}
