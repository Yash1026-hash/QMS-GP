using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Supervisor;

public class DeviationReviewModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int? SearchId { get; set; }

    public DeviationRequestDto? Deviation { get; set; }

    [BindProperty]
    public DeviationApproval Approval { get; set; } = new();

    public void OnGet()
    {
        // API integration will be added later.
    }

    public void OnPostApprove()
    {
        Approval.Decision = 1;

        // API integration will be added later.
    }

    public void OnPostReject()
    {
        Approval.Decision = 2;

        // API integration will be added later.
    }
}