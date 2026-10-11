using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;
using System.Net.Http.Json;

namespace QMSSystem.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public int TotalDeviations { get; set; }
    public int PendingDeviations { get; set; }
    public int ActiveDeviations { get; set; }
    public int InactiveDeviations { get; set; }
    public int ApprovedDeviations { get; set; }
    public int RejectedDeviations { get; set; }

    public int TotalDocuments { get; set; }
    public int TotalChangeRequests { get; set; }

    public List<DeviationRequestDto> RecentDeviations { get; set; } = [];
    public List<Document> RecentDocuments { get; set; } = [];
    public List<OperatorChangeRequest> RecentChangeRequests { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("QMSApi");
            var stats = await client.GetFromJsonAsync<AdminDashboardStatsDto>("api/admin/stats");
            if (stats != null)
            {
                TotalDeviations = stats.TotalDeviations;
                PendingDeviations = stats.PendingDeviations;
                ActiveDeviations = stats.ActiveDeviations;
                InactiveDeviations = stats.InactiveDeviations;
                ApprovedDeviations = stats.ApprovedDeviations;
                RejectedDeviations = stats.RejectedDeviations;
                TotalDocuments = stats.TotalDocuments;
                TotalChangeRequests = stats.TotalChangeRequests;
                RecentDeviations = stats.RecentDeviations ?? [];
                RecentDocuments = stats.RecentDocuments ?? [];
                RecentChangeRequests = stats.RecentChangeRequests ?? [];
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load admin dashboard stats");
            ErrorMessage = "Failed to load dashboard data from API.";
        }
    }
}
