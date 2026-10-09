using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin.Deviations;

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

    public DeviationRequestDto? Deviation { get; set; }

    public List<DeviationReportDto> Reports { get; set; } = [];

    public List<OperatorChangeRequest> LinkedChangeRequests { get; set; } = [];

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id <= 0)
        {
            return RedirectToPage("/Admin/Deviations/Index");
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var result = await client.GetFromJsonAsync<AdminDeviationDetailDto>($"api/admin/deviations/{Id}");

            if (result != null)
            {
                Deviation = result.Deviation;
                Reports = result.Reports ?? [];
                LinkedChangeRequests = result.LinkedChangeRequests ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deviation details for ID {Id}", Id);
            ErrorMessage = $"Unable to load deviation #{Id} from API.";
        }

        return Page();
    }
}
