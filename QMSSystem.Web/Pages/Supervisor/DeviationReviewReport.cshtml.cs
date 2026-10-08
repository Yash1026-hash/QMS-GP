using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos.Deviations;

namespace QMSSystem.Web.Pages.Supervisor;

public class DeviationReviewReportModel : PageModel
{
    [BindProperty]
    public DeviationReportDto Report { get; set; } = new();

    [BindProperty]
    public int? Decision { get; set; }

    [BindProperty]
    public string DecisionComments { get; set; } = string.Empty;

    public bool IsApproved =>
        Report.Decision.HasValue &&
        Report.Decision.Value == 1;

    public bool IsRejected =>
        Report.Decision.HasValue &&
        Report.Decision.Value == 2;

    public void OnGet(
        int deviationId = 0,
        int documentId = 0,
        int attemptNumber = 1)
    {
        Report.DeviationId = deviationId;
        Report.DocumentId = documentId;
        Report.AttemptNumber = attemptNumber;

        // Initial report state.
        Report.Status = 0;

        // No final approval yet.
        Report.Decision = null;
        Report.DecisionBy = string.Empty;
        Report.DecisionComments = string.Empty;
        Report.DecisionOn = null;
    }

    public IActionResult OnPost()
    {
        // Final approval is mandatory.
        if (Decision != 1 && Decision != 2)
        {
            ModelState.AddModelError(
                nameof(Decision),
                "Final approval decision is mandatory. Please select Approve or Reject.");

            return Page();
        }

        // Keep the selected decision in the report.
        Report.Decision = Decision;

        // Supervisor who made the decision.
        Report.DecisionBy =
            User.Identity?.Name ?? "Supervisor";

        // Date/time of final decision.
        Report.DecisionOn = DateTime.UtcNow;

        // Supervisor comments.
        Report.DecisionComments =
            DecisionComments?.Trim() ?? string.Empty;

        // Mark report as active after final approval.
        if (Decision == 1)
        {
            Report.Status = 1;
        }

        return Page();
    }
}