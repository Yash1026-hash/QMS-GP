using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Operator.Deviations;

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
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        if (Id <= 0)
        {
            ErrorMessage = "No deviation ID provided.";
            return;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var response = await client.GetAsync($"api/Deviation/{Id}");
            if (response.IsSuccessStatusCode)
            {
                Deviation = await response.Content.ReadFromJsonAsync<DeviationRequestDto>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                ErrorMessage = $"Deviation #{Id} was not found.";
            }
            else
            {
                ErrorMessage = "Failed to load deviation details from the server.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load deviation #{Id}", Id);
            ErrorMessage = "Could not connect to API server.";
        }
    }
}