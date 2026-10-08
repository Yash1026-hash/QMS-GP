using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QMSSystem.Shared.Dtos;
using QMSSystem.Shared.DTOs;

namespace QMSSystem.Web.Pages.Admin.ChangeRequests;



public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public OperatorChangeRequest? ChangeRequest { get; set; }

    public List<OperatorChangeRequestDeviation> LinkedDeviations { get; set; } = [];

    public IActionResult OnGet()
    {
        // Ready for API / database retrieval
        return Page();
    }
}

