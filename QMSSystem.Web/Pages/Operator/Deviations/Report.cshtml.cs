using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace QMSSystem.Web.Pages.Operator.Deviations;

public sealed class ReportModel : PageModel
{
    public int Id { get; } = 0;

    [BindProperty(SupportsGet = true)]
    public int? DeviationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? DocumentId { get; set; }

    public int AttemptNumber { get; private set; } = 1;

    [BindProperty]
    public string Summary { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Proof { get; set; }

    public string CreatedBy { get; private set; } = "Anonymous";

    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    public int Status { get; private set; }

    public void OnGet()
    {
        CreatedBy = User.Identity?.IsAuthenticated == true
            ? User.Identity.Name ?? "Unknown"
            : "Anonymous";
        CreatedDate = DateTime.UtcNow;
    }
}
