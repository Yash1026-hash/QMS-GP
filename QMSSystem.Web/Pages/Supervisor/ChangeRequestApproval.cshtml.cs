using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Supervisor;

public class ChangeRequestApprovalModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public List<OperatorChangeRequest> ChangeRequests { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public ChangeRequestApprovalModel(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");

            var response = await client.GetAsync(
                "api/ChangeRequestApproval/pending");

            if (response.IsSuccessStatusCode)
            {
                ChangeRequests =
                    await response.Content
                        .ReadFromJsonAsync<List<OperatorChangeRequest>>()
                    ?? new List<OperatorChangeRequest>();
            }
            else
            {
                ErrorMessage =
                    "Unable to load pending change requests.";
            }
        }
        catch (Exception)
        {
            ErrorMessage =
                "Unable to connect to the QMS API.";
        }
    }
}