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

    [BindProperty]
    public int AttemptNumber { get; set; } = 1;

    [BindProperty]
    public string Summary { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Proof { get; set; }

    public string CreatedBy { get; private set; } = string.Empty;

    public DateTime CreatedDate { get; private set; }

    public int Status { get; } = 0;

    public void OnGet()
    {
        CreatedBy = User.Identity?.Name ?? string.Empty;
        CreatedDate = DateTime.UtcNow;
    }
}
