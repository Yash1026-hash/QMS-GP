using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Operator.ChangeRequests
{
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public DetailsModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public OperatorChangeRequest Dto { get; set; } = new();

        public async Task OnGetAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("ApiClient");

            var response = await client.GetAsync($"api/ChangeRequest/{id}");

            if (response.IsSuccessStatusCode)
            {
                Dto = await response.Content.ReadFromJsonAsync<OperatorChangeRequest>()
                    ?? new OperatorChangeRequest();
            }
        }
    }
}