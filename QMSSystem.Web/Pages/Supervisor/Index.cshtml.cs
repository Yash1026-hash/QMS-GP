using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Supervisor;

public class IndexModel : PageModel
{
    // Pending counts
    public int PendingDeviations { get; set; } = 3;

    public int PendingChangeRequests { get; set; } = 4;

    public int PendingReports { get; set; } = 2;

    public int PendingDocuments { get; set; } = 5;

    public int TotalPending { get; set; }


    public void OnGet()
    {
        TotalPending =
            PendingDeviations +
            PendingChangeRequests +
            PendingReports +
            PendingDocuments;
    }
}