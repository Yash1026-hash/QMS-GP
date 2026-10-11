using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.Pages.Operator.ChangeRequestIndex
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public List<OperatorChangeRequest> ChangeRequests { get; set; } = new();

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("QMSApi");
                var response = await client.GetAsync("api/ChangeRequestsIndex");

                if (response.IsSuccessStatusCode)
                {
                    ChangeRequests = await response.Content
                        .ReadFromJsonAsync<List<OperatorChangeRequest>>()
                        ?? new List<OperatorChangeRequest>();
                }
            }
            catch
            {
                ChangeRequests = new List<OperatorChangeRequest>();
            }
        }
    }
}