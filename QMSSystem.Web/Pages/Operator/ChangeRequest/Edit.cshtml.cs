using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.Dtos.Deviations;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Operator.ChangeRequest;

public class EditModel : PageModel
{
    private readonly HttpClient _httpClient;

    public EditModel(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ApiClient");
    }

    [BindProperty]
    public int DocumentId { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string ChangeType { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public int RequestedByUserId { get; set; }

    [BindProperty]
    public DateTime RequestedDate { get; set; }

    [BindProperty]
    public List<int> SelectedDeviationIds { get; set; } = [];

    public List<DeviationRequestDto> Deviations { get; set; } = [];

    public async Task OnGetAsync(int documentId)
    {
        DocumentId = documentId;

        // Replace later with logged-in user
        RequestedByUserId = 1;

        RequestedDate = DateTime.Today;

        try
        {
            Deviations = await _httpClient
                .GetFromJsonAsync<List<DeviationRequestDto>>
                (
                    $"api/ChangeRequestDocument/{documentId}/deviations"
                )
                ?? new List<DeviationRequestDto>();
        }
        catch
        {
            Deviations = new List<DeviationRequestDto>();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var request = new OperatorChangeRequest
        {
            DocumentId = DocumentId,
            Title = Title,
            ChangeType = ChangeType,
            Description = Description,
            RequestedByUserId = RequestedByUserId,
            RequestedDate = RequestedDate,
            Status = "Pending"
        };

        var response =
            await _httpClient.PostAsJsonAsync(
                "api/ChangeRequest",
                request);

        if (!response.IsSuccessStatusCode)
        {
            return Page();
        }

        var changeRequestId =
            await response.Content.ReadFromJsonAsync<int>();

        if (changeRequestId <= 0)
        {
            return Page();
        }

        foreach (var deviationId in SelectedDeviationIds)
        {
            var mapping =
                new OperatorChangeRequestDeviation
                {
                    ChangeRequestId = changeRequestId,
                    DeviationId = deviationId
                };

            await _httpClient.PostAsJsonAsync(
                "api/ChangeRequest/deviation",
                mapping);
        }

        return RedirectToPage("./Documents");
    }
}