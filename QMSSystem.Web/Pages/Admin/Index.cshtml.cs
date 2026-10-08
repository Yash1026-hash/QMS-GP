using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;
using QMSSystem.Shared.Dtos;

namespace QMSSystem.Web.Pages.Admin;

public class IndexModel : PageModel
{
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

    public void OnGet()
    {
        // Ready for database/API repository integration
        TotalDeviations = RecentDeviations.Count;
        PendingDeviations = RecentDeviations.Count(d => d.Status == 0);
        ActiveDeviations = RecentDeviations.Count(d => d.Status == 1);
        InactiveDeviations = RecentDeviations.Count(d => d.Status == 2);
        ApprovedDeviations = RecentDeviations.Count(d => d.Decision == 1);
        RejectedDeviations = RecentDeviations.Count(d => d.Decision == 2);

        TotalDocuments = RecentDocuments.Count;
        TotalChangeRequests = RecentChangeRequests.Count;
    }
}
