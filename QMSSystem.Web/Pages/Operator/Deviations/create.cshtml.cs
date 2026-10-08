using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Models;

namespace QMSSystem.Web.Pages.Operator.Deviations;

[AllowAnonymous]
public class CreateModel : PageModel
{
    [BindProperty]
    public DeviationRequest Deviation { get; set; } = new();

    public string? Message { get; private set; }

    public bool IsFormValid { get; private set; }

    public IActionResult OnGet(int documentId = 0)
    {
        Deviation = CreateNewRequest(documentId);
        return Page();
    }

    public IActionResult OnPost()
    {
        if (Deviation.DocumentId <= 0)
        {
            ModelState.AddModelError("Deviation.DocumentId", "Select an active SOP before creating a deviation.");
        }

        if (string.IsNullOrWhiteSpace(Deviation.Title))
        {
            ModelState.AddModelError("Deviation.Title", "Enter a deviation title.");
        }

        if (string.IsNullOrWhiteSpace(Deviation.Description))
        {
            ModelState.AddModelError("Deviation.Description", "Describe the deviation.");
        }

        if (Deviation.Priority is not ("Minor" or "Major" or "Critical"))
        {
            ModelState.AddModelError("Deviation.Priority", "Select a valid priority.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Deviation.Id = 0;
        Deviation.Status = 0;
        Deviation.CreatedBy = User.Identity?.Name ?? string.Empty;
        Deviation.CreatedDate = DateTime.UtcNow;
        Deviation.Decision = null;
        Deviation.DecisionBy = string.Empty;
        Deviation.DecisionOn = null;
        Deviation.DecisionComments = string.Empty;

        IsFormValid = true;
        Message = "The deviation form is valid, but it was not saved because deviation persistence is not connected yet.";
        return Page();
    }

    private DeviationRequest CreateNewRequest(int documentId)
    {
        return new DeviationRequest
        {
            DocumentId = documentId,
            Status = 0,
            CreatedBy = User.Identity?.Name ?? string.Empty,
            CreatedDate = DateTime.UtcNow,
            DecisionBy = string.Empty,
            DecisionComments = string.Empty
        };
    }
}