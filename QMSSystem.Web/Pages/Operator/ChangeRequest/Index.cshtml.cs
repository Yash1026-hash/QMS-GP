using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Json;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.Pages.Operator.ChangeRequestIndex
{
    public class IndexModel : PageModel
    {
        public List<OperatorChangeRequest> ChangeRequests { get; set; } =
            new List<OperatorChangeRequest>();

        public async Task OnGetAsync()
        {
            using var client = new HttpClient();

            var authCookie = Request.Cookies["QMS.Auth"];

            if (!string.IsNullOrEmpty(authCookie))
            {
                client.DefaultRequestHeaders.Add(
                    "Cookie",
                    $"QMS.Auth={authCookie}"
                );
            }

            var response = await client.GetAsync(
                "http://localhost:5070/api/ChangeRequestsIndex"
            );

            if (response.IsSuccessStatusCode)
            {
                ChangeRequests = await response.Content
                    .ReadFromJsonAsync<List<OperatorChangeRequest>>()
                    ?? new List<OperatorChangeRequest>();
            }
        }
    }
}